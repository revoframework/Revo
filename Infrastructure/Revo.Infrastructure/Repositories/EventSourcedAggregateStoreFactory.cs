using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Repositories
{
    internal class EventSourcedAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func;

        public EventSourcedAggregateStoreFactory(Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func)
        {
            this.func = func;
        }

        public IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EventSourcedAggregateStore(func(unitOfWork.EventBuffer));
        }
    }
}
