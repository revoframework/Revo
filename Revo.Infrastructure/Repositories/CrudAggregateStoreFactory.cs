using System;
using System.Linq;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Repositories
{
    public class CrudAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly Func<ICrudRepository> crudRepositoryFunc;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;

        public CrudAggregateStoreFactory(Func<ICrudRepository> crudRepositoryFunc,
            IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory)
        {
            this.crudRepositoryFunc = crudRepositoryFunc;
            this.entityTypeManager = entityTypeManager;
            this.eventMessageFactory = eventMessageFactory;
        }

        public bool CanHandleAggregateType(Type aggregateType)
        {
            return aggregateType.GetCustomAttributes(typeof(DatabaseEntityAttribute), true).Any();
        }

        public virtual IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new CrudAggregateStore(crudRepositoryFunc(), entityTypeManager,
                unitOfWork?.EventBuffer, eventMessageFactory);
        }
    }
}
