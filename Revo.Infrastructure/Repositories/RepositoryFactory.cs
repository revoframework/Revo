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

        public IRepository CreateRepository(IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            return new Repository(aggregateStoreFactories, unitOfWorkAccessor);
        }
    }
}
