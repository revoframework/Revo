using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;
using Revo.Infrastructure.EventStores.Generic.Model;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async.Generic
{
    public class AsyncEventQueueManagerTests
    {
        private AsyncEventQueueManager sut;
        private InMemoryCrudRepository crudRepository;
        private IQueuedAsyncEventMessageFactory queuedAsyncEventMessageFactory;
        private IExternalEventStore externalEventStore;

        public AsyncEventQueueManagerTests()
        {
            crudRepository = new InMemoryCrudRepository();
            externalEventStore = Substitute.For<IExternalEventStore>();
            queuedAsyncEventMessageFactory = Substitute.For<IQueuedAsyncEventMessageFactory>();

            sut = new AsyncEventQueueManager(crudRepository, queuedAsyncEventMessageFactory, externalEventStore);
        }

        [Fact]
        public async Task EnqueueEventAsync_EventStore()
        {
            var eventStoreRecord = Substitute.For<IEventStreamRowEventStoreRecord>();
            Guid eventId = Guid.NewGuid();
            eventStoreRecord.EventStreamRow.Returns(
                new EventStreamRow(eventId, "{}", "TestEvent", 0, Guid.NewGuid(), 0,
                    DateTimeOffset.Now, "{}"));
            eventStoreRecord.EventId.Returns(eventId);

            var eventMessage = EventStoreEventMessage.FromRecord(eventStoreRecord);

            var resultEventMessage = eventMessage.Clone();
            queuedAsyncEventMessageFactory.CreateEventMessage(
                    Arg.Is<QueuedAsyncEvent>(x => x.EventStreamRow == eventStoreRecord.EventStreamRow))
                .Returns(resultEventMessage);

            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();

            var queues = crudRepository.FindAll<AsyncEventQueue>().ToArray();
            queues.Should().HaveCount(1);
            queues[0].Id.Should().Be("queue1");

            var queuedEvents = crudRepository.FindAll<QueuedAsyncEvent>().ToArray();
            queuedEvents.Should().HaveCount(1);
            queuedEvents[0].Id.Should().NotBe(Guid.Empty);
            queuedEvents[0].EventStreamRow.Should().Be(eventStoreRecord.EventStreamRow);
            queuedEvents[0].QueueId.Should().Be(queues[0].Id);
            queuedEvents[0].SequenceNumber.Should().Be(null);

            result.Should().HaveCount(1);
            result.First().EventMessage.Should().Be(resultEventMessage);
            result.First().EventId.Should().Be(eventId);
            result.First().Id.Should().NotBeEmpty();
            result.First().SequenceNumber.Should().BeNull();
            result.First().QueueName.Should().Be(queues[0].Id);

            eventStoreRecord.EventStreamRow.IsDispatchedToAsyncQueues.Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueEventAsync_EventStore_AttachesRecord()
        {
            var eventStoreRecord = Substitute.For<IEventStreamRowEventStoreRecord>();
            Guid eventId = Guid.NewGuid();
            eventStoreRecord.EventStreamRow.Returns(
                new EventStreamRow(eventId, "{}", "TestEvent", 0, Guid.NewGuid(), 0,
                    DateTimeOffset.Now, "{}"));
            eventStoreRecord.EventId.Returns(eventId);

            var eventMessage = EventStoreEventMessage.FromRecord(eventStoreRecord);
            
            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            await sut.CommitAsync();
            
            eventStoreRecord.EventStreamRow.IsDispatchedToAsyncQueues.Should().BeTrue();
            crudRepository.GetEntityState(eventStoreRecord.EventStreamRow).Should().NotBe(EntityState.Detached);
        }

        [Fact]
        public async Task EnqueueEventAsync_EventStore_SkipsAlreadyDispatched()
        {
            var eventStoreRecord = Substitute.For<IEventStreamRowEventStoreRecord>();
            Guid eventId = Guid.NewGuid();
            eventStoreRecord.EventStreamRow.Returns(
                new EventStreamRow(eventId, "{}", "TestEvent", 0, Guid.NewGuid(), 0,
                    DateTimeOffset.Now, "{}"));
            eventStoreRecord.EventId.Returns(eventId);
            eventStoreRecord.EventStreamRow.MarkDispatchedToAsyncQueues();

            var eventMessage = EventStoreEventMessage.FromRecord(eventStoreRecord);
            
            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();

            result.Should().BeEmpty();
            crudRepository.FindAll<QueuedAsyncEvent>().Should().BeEmpty();
            eventStoreRecord.EventStreamRow.IsDispatchedToAsyncQueues.Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueEventAsync_ExternalEvent()
        {
            var eventMessage = EventMessageDraft
                .FromEvent(new TestEvent())
                .SetId(Guid.NewGuid());
            
            var externalEventRecord = new ExternalEventRecord(eventMessage.Metadata.GetEventId().Value,
                "{}", "TestEvent", 1, "{}");
            externalEventStore.CommitAsync()
                .Returns(ci =>
                {
                    externalEventStore.Received(1).TryPushEvent(eventMessage);
                    return new[] {externalEventRecord};
                });

            var resultEventMessage = eventMessage.Clone();
            queuedAsyncEventMessageFactory.CreateEventMessage(
                    Arg.Is<QueuedAsyncEvent>(x => x.ExternalEventRecord == externalEventRecord))
                .Returns(resultEventMessage);

            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();

            var queues = crudRepository.FindAll<AsyncEventQueue>().ToArray();
            queues.Should().HaveCount(1);
            queues[0].Id.Should().Be("queue1");

            var queuedEvents = crudRepository.FindAll<QueuedAsyncEvent>().ToArray();
            queuedEvents.Should().HaveCount(1);
            queuedEvents[0].Id.Should().NotBe(Guid.Empty);
            queuedEvents[0].ExternalEventRecord.Should().Be(externalEventRecord);
            queuedEvents[0].QueueId.Should().Be(queues[0].Id);
            queuedEvents[0].SequenceNumber.Should().Be(null);

            result.Should().HaveCount(1);
            result.First().EventMessage.Should().Be(resultEventMessage);
            result.First().EventId.Should().Be(externalEventRecord.Id);
            result.First().Id.Should().NotBeEmpty();
            result.First().SequenceNumber.Should().BeNull();
            result.First().QueueName.Should().Be(queues[0].Id);
            
            externalEventRecord.IsDispatchedToAsyncQueues.Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueEventAsync_ExternalEvent_WithoutId()
        {
            var eventMessage = EventMessageDraft
                .FromEvent(new TestEvent());

            Guid? eventId = null;
            externalEventStore
                .When(x => x.TryPushEvent(Arg.Is<IEventMessage>(arg => arg.Event == eventMessage.Event)))
                .Do(ci => eventId = ci.Arg<IEventMessage>().Metadata.GetEventId());

            ExternalEventRecord externalEventRecord = null;
            
            externalEventStore.CommitAsync()
                .Returns(ci =>
                {
                    externalEventStore.ReceivedWithAnyArgs(1).TryPushEvent(null);
                    eventId.Should().NotBeNull();

                    externalEventRecord = new ExternalEventRecord(eventId.Value,
                        "{}", "TestEvent", 1, "{}");

                    return new[] { externalEventRecord };
                });

            var resultEventMessage = eventMessage.Clone();
            queuedAsyncEventMessageFactory.CreateEventMessage(
                    Arg.Is<QueuedAsyncEvent>(x => x.ExternalEventRecord == externalEventRecord))
                .Returns(resultEventMessage);

            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();

            var queues = crudRepository.FindAll<AsyncEventQueue>().ToArray();
            queues.Should().HaveCount(1);
            queues[0].Id.Should().Be("queue1");

            var queuedEvents = crudRepository.FindAll<QueuedAsyncEvent>().ToArray();
            queuedEvents.Should().HaveCount(1);
            queuedEvents[0].Id.Should().NotBe(Guid.Empty);
            externalEventRecord.Should().NotBeNull();
            queuedEvents[0].ExternalEventRecord.Should().Be(externalEventRecord);
            queuedEvents[0].QueueId.Should().Be(queues[0].Id);
            queuedEvents[0].SequenceNumber.Should().Be(null);

            result.Should().HaveCount(1);
            result.First().EventMessage.Should().Be(resultEventMessage);
            result.First().EventId.Should().Be(externalEventRecord.Id);
            result.First().Id.Should().NotBeEmpty();
            result.First().SequenceNumber.Should().BeNull();
            result.First().QueueName.Should().Be(queues[0].Id);
        }

        [Fact]
        public async Task EnqueueEventAsync_ExternalEvent_AttachesRecord()
        {
            var eventMessage = EventMessageDraft
                .FromEvent(new TestEvent())
                .SetId(Guid.NewGuid());

            var externalEventRecord = new ExternalEventRecord(eventMessage.Metadata.GetEventId().Value,
                "{}", "TestEvent", 1, "{}");
            externalEventStore.CommitAsync()
                .Returns(ci =>
                {
                    externalEventStore.Received(1).TryPushEvent(eventMessage);
                    return new[] { externalEventRecord };
                });
            
            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            await sut.CommitAsync();
            
            externalEventRecord.IsDispatchedToAsyncQueues.Should().BeTrue();
            crudRepository.GetEntityState(externalEventRecord).Should().NotBe(EntityState.Detached);
        }

        [Fact]
        public async Task EnqueueEventAsync_ExternalEvent_SkipsAlreadyDispatched()
        {
            var eventMessage = EventMessageDraft
                .FromEvent(new TestEvent())
                .SetId(Guid.NewGuid());

            var externalEventRecord = new ExternalEventRecord(eventMessage.Metadata.GetEventId().Value,
                "{}", "TestEvent", 1, "{}");
            externalEventRecord.MarkDispatchedToAsyncQueues();
            externalEventStore.CommitAsync()
                .Returns(ci =>
                {
                    externalEventStore.Received(1).TryPushEvent(eventMessage);
                    return new[] { externalEventRecord };
                });
            
            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();

            result.Should().BeEmpty();
            crudRepository.FindAll<QueuedAsyncEvent>().Should().BeEmpty();
            externalEventRecord.Should().NotBeNull();
            externalEventRecord.IsDispatchedToAsyncQueues.Should().BeTrue();
        }

        [Fact]
        public async Task EnqueueEventAsync_ExternalEvent_AnotherEnqueueCalledFromCommit()
        {
            // This test is important for providers like EF Core that use coordinated transaction for committing multiple things at once.
            // Because of this, AsyncEventQueueManager.CommitAsync may get invoked again from the transaction coordinator during ExternalEventStore.CommitAsync,
            // in which case we must make sure the events don't get enqueued twice.

            var eventMessage = EventMessageDraft
                .FromEvent(new TestEvent());

            Guid? eventId = null;
            externalEventStore
                .When(x => x.TryPushEvent(Arg.Is<IEventMessage>(arg => arg.Event == eventMessage.Event)))
                .Do(ci => eventId = ci.Arg<IEventMessage>().Metadata.GetEventId());

            ExternalEventRecord externalEventRecord = null;

            int storeCommitsRunning = 0;

            externalEventStore.CommitAsync()
                .Returns(async ci =>
                {
                    externalEventStore.ReceivedWithAnyArgs(1).TryPushEvent(null);
                    eventId.Should().NotBeNull();

                    externalEventRecord = new ExternalEventRecord(eventId.Value,
                        "{}", "TestEvent", 1, "{}");
                    
                    if (storeCommitsRunning == 0)
                    {
                        storeCommitsRunning++;
                        try
                        {
                            await sut.CommitAsync();
                        }
                        finally
                        {
                            storeCommitsRunning--;
                        }
                    }

                    return new[] { externalEventRecord };
                });

            var resultEventMessage = eventMessage.Clone();
            queuedAsyncEventMessageFactory.CreateEventMessage(
                    Arg.Is<QueuedAsyncEvent>(x => x.ExternalEventRecord == externalEventRecord))
                .Returns(resultEventMessage);

            await sut.EnqueueEventAsync(eventMessage, new[]
            {
                new EventSequencing() {SequenceName = "queue1"}
            });
            var result = await sut.CommitAsync();
            
            var queuedEvents = crudRepository.FindAll<QueuedAsyncEvent>().ToArray();
            queuedEvents.Should().HaveCount(1);
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task DequeueEventAsync()
        {
            var queue = new AsyncEventQueue("queue1", null);
            crudRepository.Attach(queue);
            var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                    Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1);
            crudRepository.Attach(queuedEvent);

            await sut.DequeueEventAsync(queuedEvent.Id);
            var result = await sut.CommitAsync();

            result.Should().BeEmpty();
            crudRepository.FindAll<AsyncEventQueue>().Should().BeEquivalentTo(new[] { queue });
            crudRepository.FindAll<QueuedAsyncEvent>().Should().BeEmpty();
            queue.LastSequenceNumberProcessed.Should().Be(1);
        }

        [Fact]
        public async Task GetQueueEventsAsync()
        {
            var queue = new AsyncEventQueue("queue1", null);
            crudRepository.Attach(queue);
            var queuedEvent = new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                    Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1);
            crudRepository.Attach(queuedEvent);

            var eventMessage = EventMessageDraft.FromEvent(new TestEvent())
                .SetId(queuedEvent.EventStreamRow.Id);
            queuedAsyncEventMessageFactory.CreateEventMessage(queuedEvent)
                .Returns(eventMessage);

            var result = await sut.GetQueueEventsAsync("queue1");

            result.Should().HaveCount(1);
            result.First().EventMessage.Should().Be(eventMessage);
            result.First().EventId.Should().Be(queuedEvent.EventStreamRow.Id);
            result.First().Id.Should().Be(queuedEvent.Id);
            result.First().SequenceNumber.Should().Be(1);
            result.First().QueueName.Should().Be("queue1");
        }

        [Fact]
        public async Task GetNonemptyQueueNamesAsync()
        {
            var queuedEvents = new[]
                {
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1),
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1),
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue2",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1)

                };
            crudRepository.AttachRange(queuedEvents);
            
            var result = await sut.GetNonemptyQueueNamesAsync();
            result.Should().BeEquivalentTo("queue1", "queue2");
        }

        [Fact]
        public async Task FindQueuedEventsAsync()
        {
            var queuedEvents = new[]
                {
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1),
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue1",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1),
                    new QueuedAsyncEvent(Guid.NewGuid(), "queue2",
                        new EventStreamRow(Guid.NewGuid(), "{}", "TestEvent", 1,
                            Guid.NewGuid(), 1, DateTimeOffset.Now, "{}"), 1)

                };
            crudRepository.AttachRange(queuedEvents);

            var eventMessage = EventMessageDraft.FromEvent(new TestEvent())
                .SetId(queuedEvents[2].EventStreamRow.Id);
            queuedAsyncEventMessageFactory.CreateEventMessage(queuedEvents[2])
                .Returns(eventMessage);

            var result = await sut.FindQueuedEventsAsync(new[]
            {
                queuedEvents[2].Id
            });

            result.Should().HaveCount(1);
            result.First().EventMessage.Should().Be(eventMessage);
            result.First().EventId.Should().Be(queuedEvents[2].EventStreamRow.Id);
            result.First().Id.Should().Be(queuedEvents[2].Id);
            result.First().SequenceNumber.Should().Be(1);
            result.First().QueueName.Should().Be("queue2");
        }

        [Fact]
        public async Task GetQueueStateAsync()
        {
            var queue = new AsyncEventQueue("queue1", 1);
            crudRepository.Attach(queue);

            var result = await sut.GetQueueStateAsync("queue1");
            result.LastSequenceNumberProcessed.Should().Be(1);
        }

        public class TestEvent : IEvent
        {
        }
    }
}
