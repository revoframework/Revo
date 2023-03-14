using System;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreEventSourcedAggregateStoreFactory : EventSourcedAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, IEFCoreTransactionCoordinator, EFCoreEventSourcedAggregateStore> func;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventSourcedAggregateStoreFactory(
            Func<IPublishEventBuffer, IEFCoreTransactionCoordinator, EFCoreEventSourcedAggregateStore> func,
            IEFCoreTransactionCoordinator transactionCoordinator) : base(null)
        {
            this.func = func;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return func(unitOfWork?.EventBuffer, transactionCoordinator);
        }
    }
}
