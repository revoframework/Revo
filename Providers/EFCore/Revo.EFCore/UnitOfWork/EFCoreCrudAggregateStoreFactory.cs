using System;
using System.Collections.Generic;
using System.Text;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.UnitOfWork
{
    public class EFCoreCrudAggregateStoreFactory : CrudAggregateStoreFactory
    {
        private readonly Func<ICrudRepository> crudRepositoryFunc;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreCrudAggregateStoreFactory(Func<ICrudRepository> crudRepositoryFunc,
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
                unitOfWork.EventBuffer, eventMessageFactory, transactionCoordinator);
        }
    }
}
