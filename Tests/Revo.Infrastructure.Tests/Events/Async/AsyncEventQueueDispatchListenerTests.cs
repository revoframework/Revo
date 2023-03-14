using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Events.Async;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventQueueDispatchListenerTests
    {
        private AsyncEventQueueDispatchListener sut;
        private IAsyncEventQueueDispatcher asyncEventQueueDispatcher;
        private IAsyncEventProcessor asyncEventProcessor;
        private List<IEventMessage<IEvent>> messages;
        private IUnitOfWork unitOfWork;

        public AsyncEventQueueDispatchListenerTests()
        {
            asyncEventQueueDispatcher = Substitute.For<IAsyncEventQueueDispatcher>();
            asyncEventProcessor = Substitute.For<IAsyncEventProcessor>();

            sut = new AsyncEventQueueDispatchListener(asyncEventQueueDispatcher, asyncEventProcessor);

            messages = new List<IEventMessage<IEvent>>()
            {
                new EventMessage<Event1>(new Event1(), new Dictionary<string, string>()),
                new EventMessage<Event1>(new Event1(), new Dictionary<string, string>())
            };

            unitOfWork = Substitute.For<IUnitOfWork>();
        }

        [Fact]
        public async Task OnWorkSucceededAsync_Dispatches()
        {
            var dispatchResult = new QueueDispatchResult()
            {
                EnqueuedEventsAsyncProcessed = new List<IAsyncEventQueueRecord>() {},
                EnqueuedEventsSyncProcessed = new List<IAsyncEventQueueRecord>() {}
            };
            asyncEventQueueDispatcher.DispatchToQueuesAsync(null, null, null).ReturnsForAnyArgs(dispatchResult);

            IEnumerable<IEventMessage> dispatchedEvents = new List<IEventMessage>();
            asyncEventQueueDispatcher.WhenForAnyArgs(x => x.DispatchToQueuesAsync(null, null, null))
                .Do(ci =>
                {
                    dispatchedEvents = ci.ArgAt<IEnumerable<IEventMessage>>(0).ToList();
                });

            await sut.HandleAsync(messages[0], CancellationToken.None);
            await sut.HandleAsync(messages[1], CancellationToken.None);
            await sut.OnWorkSucceededAsync(unitOfWork);

            asyncEventQueueDispatcher.Received(1).DispatchToQueuesAsync(Arg.Any<IEnumerable<IEventMessage>>(), null, null);
            dispatchedEvents.Should().BeEquivalentTo(messages);
            asyncEventProcessor.DidNotReceiveWithAnyArgs().EnqueueForAsyncProcessingAsync(null, null);
            asyncEventProcessor.DidNotReceiveWithAnyArgs().ProcessSynchronously(null);
        }

        [Fact]
        public async Task OnWorkSucceededAsync_ProcessesAsyncAndSync()
        {
            var dispatchResult = new QueueDispatchResult()
            {
                EnqueuedEventsAsyncProcessed = new List<IAsyncEventQueueRecord>()
                {
                    new FakeAsyncEventQueueRecord() { EventId = Guid.NewGuid(), EventMessage = messages[0], Id = Guid.NewGuid(), QueueName = "queue1", SequenceNumber = 1 }
                },
                EnqueuedEventsSyncProcessed = new List<IAsyncEventQueueRecord>()
                {
                    new FakeAsyncEventQueueRecord() { EventId = Guid.NewGuid(), EventMessage = messages[0], Id = Guid.NewGuid(), QueueName = "queue2", SequenceNumber = 1 }
                }
            };
            asyncEventQueueDispatcher.DispatchToQueuesAsync(null, null, null).ReturnsForAnyArgs(dispatchResult);
            
            await sut.HandleAsync(messages[0], CancellationToken.None);
            await sut.OnWorkSucceededAsync(unitOfWork);
            
            Received.InOrder(() =>
            {
                asyncEventProcessor.EnqueueForAsyncProcessingAsync(
                    Arg.Is<IReadOnlyCollection<IAsyncEventQueueRecord>>(x =>
                        x.SequenceEqual(dispatchResult.EnqueuedEventsAsyncProcessed)),
                    null);

                asyncEventProcessor.ProcessSynchronously(
                    Arg.Is<IReadOnlyCollection<IAsyncEventQueueRecord>>(x =>
                        x.SequenceEqual(dispatchResult.EnqueuedEventsSyncProcessed)));
            });
        }

        [Fact]
        public async Task OnWorkSucceededAsync_ProcessesAsync()
        {
            var dispatchResult = new QueueDispatchResult()
            {
                EnqueuedEventsAsyncProcessed = new List<IAsyncEventQueueRecord>()
                {
                    new FakeAsyncEventQueueRecord() { EventId = Guid.NewGuid(), EventMessage = messages[0], Id = Guid.NewGuid(), QueueName = "queue1", SequenceNumber = 1 }
                },
                EnqueuedEventsSyncProcessed = new List<IAsyncEventQueueRecord>() {}
            };
            asyncEventQueueDispatcher.DispatchToQueuesAsync(null, null, null).ReturnsForAnyArgs(dispatchResult);

            await sut.HandleAsync(messages[0], CancellationToken.None);
            await sut.OnWorkSucceededAsync(unitOfWork);

            asyncEventProcessor.Received(1).EnqueueForAsyncProcessingAsync(
                Arg.Is<IReadOnlyCollection<IAsyncEventQueueRecord>>(x =>
                    x.SequenceEqual(dispatchResult.EnqueuedEventsAsyncProcessed)),
                null);

            asyncEventProcessor.DidNotReceiveWithAnyArgs().ProcessSynchronously(null);
        }



        [Fact]
        public async Task OnWorkSucceededAsync_Sync()
        {
            var dispatchResult = new QueueDispatchResult()
            {
                EnqueuedEventsAsyncProcessed = new List<IAsyncEventQueueRecord>() {},
                EnqueuedEventsSyncProcessed = new List<IAsyncEventQueueRecord>()
                {
                    new FakeAsyncEventQueueRecord() { EventId = Guid.NewGuid(), EventMessage = messages[0], Id = Guid.NewGuid(), QueueName = "queue2", SequenceNumber = 1 }
                }
            };
            asyncEventQueueDispatcher.DispatchToQueuesAsync(null, null, null).ReturnsForAnyArgs(dispatchResult);

            await sut.HandleAsync(messages[0], CancellationToken.None);
            await sut.OnWorkSucceededAsync(unitOfWork);

            asyncEventProcessor.DidNotReceiveWithAnyArgs().EnqueueForAsyncProcessingAsync(null, null);

            asyncEventProcessor.Received(1).ProcessSynchronously(
                Arg.Is<IReadOnlyCollection<IAsyncEventQueueRecord>>(x =>
                    x.SequenceEqual(dispatchResult.EnqueuedEventsSyncProcessed)));
        }

        public class Event1 : IEvent
        {
        }
    }
}
