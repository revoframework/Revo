using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;

namespace Revo.Infrastructure.Repositories
{
    internal class CrudAggregateStoreFactory : IAggregateStoreFactory
    {
        private readonly ICrudRepositoryFactory<ICrudRepository> crudRepositoryFactory;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;

        public CrudAggregateStoreFactory(
            ICrudRepositoryFactory<ICrudRepository> crudRepositoryFactory,
            IEntityTypeManager entityTypeManager,
            IEventMessageFactory eventMessageFactory)
        {
            this.crudRepositoryFactory = crudRepositoryFactory;
            this.entityTypeManager = entityTypeManager;
            this.eventMessageFactory = eventMessageFactory;
        }

        public IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            var crudRepository = crudRepositoryFactory.Create();
            return new CrudAggregateStore(crudRepository, entityTypeManager, unitOfWork.EventBuffer, eventMessageFactory);
        }
    }
}
