using System;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreCrudAggregateStoreFactory : CrudAggregateStoreFactory
    {
        private readonly Func<IEFCoreCrudRepository> crudRepositoryFunc;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreCrudAggregateStoreFactory(Func<IEFCoreCrudRepository> crudRepositoryFunc,
            IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepositoryFunc, entityTypeManager, eventMessageFactory)
        {
            this.crudRepositoryFunc = crudRepositoryFunc;
            this.entityTypeManager = entityTypeManager;
            this.eventMessageFactory = eventMessageFactory;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EFCoreCrudAggregateStore(crudRepositoryFunc(), entityTypeManager,
                unitOfWork?.EventBuffer, eventMessageFactory, transactionCoordinator);
        }
    }
}
