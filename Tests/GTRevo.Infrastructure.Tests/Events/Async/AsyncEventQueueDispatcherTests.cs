using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events.Async;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventQueueDispatcherTests
    {
        private AsyncEventQueueDispatcher sut;
        private IAsyncEventQueueManager asyncEventQueueManager;
        private IServiceLocator serviceLocator;
        private List<IEventMessage> eventMessages = new List<IEventMessage>();
        private List<IAsyncEventSequencer> eventSequencers = new List<IAsyncEventSequencer>();

        public AsyncEventQueueDispatcherTests()
        {
            asyncEventQueueManager = Substitute.For<IAsyncEventQueueManager>();
            serviceLocator = Substitute.For<IServiceLocator>();

            sut = new AsyncEventQueueDispatcher(asyncEventQueueManager, serviceLocator);

            eventSequencers.Add(Substitute.For<IAsyncEventSequencer<Event1>>());
            eventSequencers.Add(Substitute.For<IAsyncEventSequencer<Event1>>());

            serviceLocator.GetAll(typeof(IAsyncEventSequencer<Event1>)).Returns(eventSequencers);

            eventMessages = new List<IEventMessage>()
            {
                new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()),
                new EventMessage<Event1>(new Event1(), new Dictionary<string, string>())
            };

            eventSequencers[0].GetEventSequencing(eventMessages[0]).Returns(new List<EventSequencing>()
            {
                new EventSequencing() {SequenceName = "queue1", EventSequenceNumber = 1},
                new EventSequencing() {SequenceName = "queue2", EventSequenceNumber = 1},
            });
            
            eventSequencers[0].GetEventSequencing(eventMessages[1]).Returns(new List<EventSequencing>()
            {
                new EventSequencing() {SequenceName = "queue1", EventSequenceNumber = 2},
                new EventSequencing() {SequenceName = "queue2", EventSequenceNumber = 2},
            });

            eventSequencers[0].ShouldAttemptSynchronousDispatch(null).ReturnsForAnyArgs(true);

            eventSequencers[1].GetEventSequencing(eventMessages[0]).Returns(new List<EventSequencing>()
            {
                new EventSequencing() {SequenceName = "queue1", EventSequenceNumber = 1}
            });

            eventSequencers[1].GetEventSequencing(eventMessages[1]).Returns(new List<EventSequencing>()
            {
                new EventSequencing() {SequenceName = "queue3", EventSequenceNumber = 3}
            });
            
            eventSequencers[1].ShouldAttemptSynchronousDispatch(eventMessages[0]).ReturnsForAnyArgs(true);
            eventSequencers[1].ShouldAttemptSynchronousDispatch(eventMessages[1]).ReturnsForAnyArgs(false);
        }

        [Fact]
        public async Task GetLastEventSourceDispatchCheckpointAsync_GetsFromManager()
        {
            asyncEventQueueManager.GetEventSourceCheckpointAsync("eventSource").Returns("checkpoint");

            string checkpoint = await sut.GetLastEventSourceDispatchCheckpointAsync("eventSource");
            checkpoint.Should().Be("checkpoint");
        }

        [Fact]
        public async Task DispatchToQueuesAsync_EnqueuesEvents()
        {
            List<EventSequencing> event1Sequencing = new List<EventSequencing>();
            List<EventSequencing> event2Sequencing = new List<EventSequencing>();

            asyncEventQueueManager.When(x => x.EnqueueEventAsync(eventMessages[0], Arg.Any<IEnumerable<EventSequencing>>()))
                .Do(ci => event1Sequencing.AddRange(ci.ArgAt<IEnumerable<EventSequencing>>(1)));
            asyncEventQueueManager.When(x => x.EnqueueEventAsync(eventMessages[1], Arg.Any<IEnumerable<EventSequencing>>()))
                .Do(ci => event2Sequencing.AddRange(ci.ArgAt<IEnumerable<EventSequencing>>(1)));

            asyncEventQueueManager.CommitAsync().Returns(new List<IAsyncEventQueueRecord>()
            {
                new FakeAsyncEventQueueRecord()
                {
                    EventId = Guid.NewGuid(), EventMessage = eventMessages[0],
                    Id = Guid.NewGuid(), QueueName = "queue2", SequenceNumber = 1
                },
                new FakeAsyncEventQueueRecord()
                {
                    EventId = Guid.NewGuid(), EventMessage = eventMessages[1],
                    Id = Guid.NewGuid(), QueueName = "queue3", SequenceNumber = 1
                }
            });

            QueueDispatchResult result = await sut.DispatchToQueuesAsync(eventMessages, "eventSource", "checkpoint");

            Received.InOrder(() =>
            {
                asyncEventQueueManager.EnqueueEventAsync(eventMessages[0], Arg.Any<IEnumerable<EventSequencing>>());
                asyncEventQueueManager.EnqueueEventAsync(eventMessages[1], Arg.Any<IEnumerable<EventSequencing>>());
                asyncEventQueueManager.CommitAsync();
            });

            Received.InOrder(() =>
            {
                asyncEventQueueManager.SetEventSourceCheckpointAsync("eventSource", "checkpoint");
                asyncEventQueueManager.CommitAsync();
            });

            asyncEventQueueManager.Received(1).CommitAsync();

            event1Sequencing.Should().HaveCount(2);
            event1Sequencing.Should().Contain(x => x.SequenceName == "queue1" && x.EventSequenceNumber == 1);
            event1Sequencing.Should().Contain(x => x.SequenceName == "queue2" && x.EventSequenceNumber == 1);

            event2Sequencing.Should().HaveCount(3);
            event2Sequencing.Should().Contain(x => x.SequenceName == "queue1" && x.EventSequenceNumber == 2);
            event2Sequencing.Should().Contain(x => x.SequenceName == "queue2" && x.EventSequenceNumber == 2);
            event2Sequencing.Should().Contain(x => x.SequenceName == "queue3" && x.EventSequenceNumber == 3);
            
            result.EnqueuedEventsAsyncProcessed.Should().HaveCount(1);
            result.EnqueuedEventsAsyncProcessed.Should().Contain(x => x.EventMessage == eventMessages[1]
                                                                      && x.QueueName == "queue3");
            result.EnqueuedEventsSyncProcessed.Should().HaveCount(1);
            result.EnqueuedEventsSyncProcessed.Should().Contain(x => x.EventMessage == eventMessages[0]
                                                                      && x.QueueName == "queue2");
        }

        public class Event1 : IEvent
        {
        }
    }
}
