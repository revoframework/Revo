using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using NSubstitute;
using Xunit;

namespace GTRevo.Platform.Tests.Events
{
    public class EventQueueTests
    {
        private readonly IEventQueue sut;
        private readonly IEventBus eventBus;
        private readonly List<IEventQueueTransactionListener> listeners = new List<IEventQueueTransactionListener>();

        public EventQueueTests()
        {
            eventBus = Substitute.For<IEventBus>();

            CreateListener();

            sut = new EventQueue(eventBus, () => listeners.ToArray());
        }

        [Fact]
        public void CreateTransaction_NotifiesListeners()
        {
            using (var tx = sut.CreateTransaction())
            {
                listeners[0].Received(1).OnTransactionBegin(tx);
            }
        }

        [Fact]
        public async Task CreateTransaction_CommitAsync_NotifiesListeners()
        {
            using (var tx = sut.CreateTransaction())
            {
                await tx.CommitAsync();

                listeners[0].Received(1).OnTransactionSucceededAsync(tx);
            }
        }

        [Fact]
        public async Task CreateTransaction_CommitAsync_PublishesEvents()
        {
            using (var tx = sut.CreateTransaction())
            {
                var ev1 = new Event1();
                var ev2 = new Event2();

                sut.PushEvent(ev1);
                sut.PushEvent(ev2);

                await tx.CommitAsync();

                eventBus.Received(1).Publish(ev1);
                eventBus.Received(1).Publish(ev2);
            }
        }

        private IEventQueueTransactionListener CreateListener()
        {
            var listener = Substitute.For<IEventQueueTransactionListener>();
            listeners.Add(listener);
            return listener;
        }

        public class Event1 : IEvent
        {
            public Guid Id { get; set; }
        }

        public class Event2 : IEvent
        {
            public Guid Id { get; set; }
        }
    }
}
