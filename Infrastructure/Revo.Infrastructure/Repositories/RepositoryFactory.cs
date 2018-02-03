using System;
using Revo.Core.Events;

namespace Revo.Infrastructure.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly Func<IAggregateStore[]> aggregateStoresFunc;
        private readonly Func<IPublishEventBuffer> eventQueueFunc;

        public RepositoryFactory(Func<IAggregateStore[]> aggregateStoresFunc,
            Func<IPublishEventBuffer> eventQueueFunc)
        {
            this.aggregateStoresFunc = aggregateStoresFunc;
            this.eventQueueFunc = eventQueueFunc;
        }

        public IRepository CreateRepository(IPublishEventBuffer eventQueue = null)
        {
            return new Repository(aggregateStoresFunc(), eventQueue ?? eventQueueFunc());
        }
    }
}
