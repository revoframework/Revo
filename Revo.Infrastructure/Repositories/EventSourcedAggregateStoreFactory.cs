using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Repositories
{
    public class EventSourcedAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func;

        public EventSourcedAggregateStoreFactory(Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func)
        {
            this.func = func;
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return typeof(IEventSourcedAggregateRoot).IsAssignableFrom(aggregateType);
        }

        public virtual IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EventSourcedAggregateStore(func(unitOfWork.EventBuffer));
        }
    }
}
