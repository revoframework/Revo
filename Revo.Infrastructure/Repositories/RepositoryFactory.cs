using System;
using System.Linq;
using Revo.Core.Events;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public class RepositoryFactory : IRepositoryFactory
    {
        private readonly IAggregateStoreFactory[] aggregateStoreFactories;

        public RepositoryFactory(IAggregateStoreFactory[] aggregateStoreFactories)
        {
            this.aggregateStoreFactories = aggregateStoreFactories;
        }

        public IRepository CreateRepository(IUnitOfWork unitOfWork)
        {
            return new Repository(aggregateStoreFactories, unitOfWork);
        }
    }
}
