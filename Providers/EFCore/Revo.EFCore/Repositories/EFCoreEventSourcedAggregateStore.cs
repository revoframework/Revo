using System.Threading.Tasks;
using Revo.Core.Transactions;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreEventSourcedAggregateStore : EventSourcedAggregateStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventSourcedAggregateStore(IEventSourcedAggregateRepository eventSourcedRepository,
            IEFCoreTransactionCoordinator transactionCoordinator) : base(eventSourcedRepository)
        {
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override bool NeedsSave => false;

        public override Task SaveChangesAsync()
        {
            return transactionCoordinator.CommitAsync();
        }

        public Task OnBeforeCommitAsync()
        {
            return base.SaveChangesAsync();
        }

        public Task OnCommitSucceededAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            // TODO do a rollback on commited aggregates?
            return Task.CompletedTask;
        }
    }
}
