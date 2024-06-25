using System;
using System.Linq;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Repositories
{
    public class CrudAggregateStoreFactory(Func<ICrudRepository> crudRepositoryFunc,
            IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory) : IAggregateStoreFactory
    {
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
