using System;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreEventSourcedAggregateStoreFactory : EventSourcedAggregateStoreFactory
    {
        private readonly Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventSourcedAggregateStoreFactory(Func<IPublishEventBuffer, IEventSourcedAggregateRepository> func,
            IEFCoreTransactionCoordinator transactionCoordinator) : base(func)
        {
            this.func = func;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EFCoreEventSourcedAggregateStore(func(unitOfWork.EventBuffer), transactionCoordinator);
        }
    }
}
