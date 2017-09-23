using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Sagas
{
    public class SagaEventListener : IEventListener<DomainEvent>,
        IEventQueueTransactionListener
    {
        private readonly ISagaLocator sagaLocator;
        private readonly List<DomainEvent> bufferedEvents = new List<DomainEvent>();

        public SagaEventListener(ISagaLocator sagaLocator)
        {
            this.sagaLocator = sagaLocator;
        }

        public async Task Handle(DomainEvent notification)
        {
            bufferedEvents.Add(notification);
        }

        public void OnTransactionBegin(ITransaction transaction)
        {
        }

        public Task OnTransactionSucceededAsync(ITransaction transaction)
        {
            return DispatchSagaEvents();
        }

        private async Task DispatchSagaEvents()
        {
            await sagaLocator.LocateAndDispatchAsync(bufferedEvents);
            bufferedEvents.Clear();
        }
    }
}
