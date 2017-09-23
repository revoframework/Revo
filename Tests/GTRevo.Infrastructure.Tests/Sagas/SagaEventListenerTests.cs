using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Sagas;
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

            sut = new SagaEventListener(sagaLocator);
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_DispatchesEvents()
        {
            var event1 = new Event1();
            var transaction = Substitute.For<ITransaction>();

            List<DomainEvent> events = null;
            sagaLocator.WhenForAnyArgs(x => x.LocateAndDispatchAsync(null))
                .Do(ci =>
                {
                    events = ci.ArgAt<IEnumerable<DomainEvent>>(0).ToList();
                });

            sut.OnTransactionBegin(transaction);
            await sut.Handle(event1);
            await sut.OnTransactionSucceededAsync(transaction);

            sagaLocator.ReceivedWithAnyArgs(1).LocateAndDispatchAsync(null);

            Assert.True(events != null && events.SequenceEqual(new List<DomainEvent>()
            {
                event1
            }));
        }

        public class Event1 : DomainEvent
        {
        }
    }
}
