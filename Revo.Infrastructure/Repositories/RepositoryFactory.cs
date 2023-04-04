using System.Collections.Generic;
using System.Linq;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories;

public class RepositoryFactory : IRepositoryFactory
{
    private readonly IAggregateStoreFactory[] aggregateStoreFactories;

    public RepositoryFactory(IEnumerable<IAggregateStoreFactory> aggregateStoreFactories)
    {
        this.aggregateStoreFactories = aggregateStoreFactories.ToArray();
    }

    public IRepository CreateRepository(IUnitOfWorkAccessor unitOfWorkAccessor)
    {
        return new Repository(aggregateStoreFactories, unitOfWorkAccessor);
    }
}