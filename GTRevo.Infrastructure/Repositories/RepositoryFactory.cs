using System;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly Func<IAggregateStore[]> aggregateStoresFunc;
        private readonly Func<IEventQueue> eventQueueFunc;

        public RepositoryFactory(Func<IAggregateStore[]> aggregateStoresFunc,
            Func<IEventQueue> eventQueueFunc)
        {
            this.aggregateStoresFunc = aggregateStoresFunc;
            this.eventQueueFunc = eventQueueFunc;
        }

        public IRepository CreateRepository(IEventQueue eventQueue = null)
        {
            return new Repository(aggregateStoresFunc(), eventQueue ?? eventQueueFunc());
        }
    }
}
