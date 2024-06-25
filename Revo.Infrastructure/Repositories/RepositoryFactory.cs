using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public class RepositoryFactory(IAggregateStoreFactory[] aggregateStoreFactories) : IRepositoryFactory
    {
        public IRepository CreateRepository(IUnitOfWorkAccessor unitOfWorkAccessor)
        {
            return new Repository(aggregateStoreFactories, unitOfWorkAccessor);
        }
    }
}
