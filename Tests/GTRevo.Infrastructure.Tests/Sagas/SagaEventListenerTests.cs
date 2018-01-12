using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Sagas;
using GTRevo.Testing.Infrastructure;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Sagas
{
    public class SagaEventListenerTests
    {
        private readonly SagaEventListener sut;
        private readonly ISagaLocator sagaLocator;

        public SagaEventListenerTests()
        {
            sagaLocator = Substitute.For<ISagaLocator>();

            sut = new SagaEventListener(sagaLocator, new SagaEventListener.SagaEventSequencer());
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_DispatchesEvents()
        {
            var event1 = new Event1().ToMessageDraft();

            List<IEventMessage<DomainEvent>> events = null;
            sagaLocator.WhenForAnyArgs(x => x.LocateAndDispatchAsync(null))
                .Do(ci =>
                {
                    events = ci.ArgAt<IEnumerable<IEventMessage<DomainEvent>>>(0).ToList();
                });
            
            await sut.HandleAsync(event1, "SagaEventListener");
            await sut.OnFinishedEventQueueAsync("SagaEventListener");

            sagaLocator.ReceivedWithAnyArgs(1).LocateAndDispatchAsync(null);

            Assert.True(events != null && events.SequenceEqual(new List<IEventMessage<DomainEvent>>()
            {
                event1
            }));
        }

        public class Event1 : DomainEvent
        {
        }
    }
}
