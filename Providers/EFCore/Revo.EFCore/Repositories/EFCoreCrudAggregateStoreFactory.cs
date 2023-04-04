using Revo.Core.Services;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreCrudAggregateStoreFactory : CrudAggregateStoreFactory
    {
        private readonly IServiceFactory<IEFCoreCrudRepository> efCoreCrudRepositoryFactory;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreCrudAggregateStoreFactory(IServiceFactory<IEFCoreCrudRepository> efCoreCrudRepositoryFactory,
            IServiceFactory<ICrudRepository> crudRepositoryFactory,
            IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepositoryFactory, entityTypeManager, eventMessageFactory)
        {
            this.efCoreCrudRepositoryFactory = efCoreCrudRepositoryFactory;
            this.entityTypeManager = entityTypeManager;
            this.eventMessageFactory = eventMessageFactory;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EFCoreCrudAggregateStore(efCoreCrudRepositoryFactory.GetRequiredService(), entityTypeManager,
                unitOfWork?.EventBuffer, eventMessageFactory, transactionCoordinator);
        }
    }
}
