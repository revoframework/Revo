using System.Threading.Tasks;
using Revo.Core.Transactions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Repositories;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EFCore.EventStores
{
    public class EFCoreEventStore : EventStore, ITransactionParticipant, IEFCoreEventStore
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventStore(IEFCoreCrudRepository crudRepository, IEventSerializer eventSerializer,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, eventSerializer)
        {
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override string EventSourceName => "EFCore.EventStore";
        
        public override async Task CommitChangesAsync()
        {
            await transactionCoordinator.CommitAsync();
        }

        public Task OnBeforeCommitAsync()
        {
            return DoBeforeCommitAsync();
        }

        public Task OnCommitSucceededAsync()
        {
            return DoOnCommitSucceedAsync();
        }

        public Task OnCommitFailedAsync()
        {
            return DoOnCommitFailedAsync();
        }
    }
}
