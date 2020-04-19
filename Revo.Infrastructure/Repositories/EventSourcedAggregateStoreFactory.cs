using System;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities.EventSourcing;

namespace Revo.Infrastructure.Repositories
{
    public class EventSourcedAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, EventSourcedAggregateStore> eventSourcedAggregateStoreFunc;

        public EventSourcedAggregateStoreFactory(Func<IPublishEventBuffer, EventSourcedAggregateStore> eventSourcedAggregateStoreFunc)
        {
            this.eventSourcedAggregateStoreFunc = eventSourcedAggregateStoreFunc;
        }
        
        public bool CanHandleAggregateType(Type aggregateType)
        {
            return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
        }

        public virtual IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return eventSourcedAggregateStoreFunc(unitOfWork?.EventBuffer);
        }
    }
}
