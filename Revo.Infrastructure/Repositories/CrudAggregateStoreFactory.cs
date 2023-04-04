using System;
using System.Linq;
using Revo.Core.Services;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Repositories;

public class CrudAggregateStoreFactory : IAggregateStoreFactory
{
    private readonly IServiceFactory<ICrudRepository> crudRepositoryFactory;
    private readonly IEntityTypeManager entityTypeManager;
    private readonly IEventMessageFactory eventMessageFactory;

    public CrudAggregateStoreFactory(IServiceFactory<ICrudRepository> crudRepositoryFactory,
        IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory)
    {
        this.crudRepositoryFactory = crudRepositoryFactory;
        this.entityTypeManager = entityTypeManager;
        this.eventMessageFactory = eventMessageFactory;
    }

    public bool CanHandleAggregateType(Type aggregateType)
    {
        return aggregateType.GetCustomAttributes(typeof(DatabaseEntityAttribute), true).Any();
    }

    public virtual IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
    {
        return new CrudAggregateStore(crudRepositoryFactory.GetRequiredService(), entityTypeManager,
            unitOfWork?.EventBuffer, eventMessageFactory);
    }
}
