using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreCrudAggregateStore : CrudAggregateStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreCrudAggregateStore(ICrudRepository crudRepository, IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer, IEventMessageFactory eventMessageFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, entityTypeManager, publishEventBuffer, eventMessageFactory)
        {
            this.transactionCoordinator = transactionCoordinator;
            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override bool NeedsSave => false;

        public override Task SaveChangesAsync()
        {
            return transactionCoordinator.CommitAsync();
        }

        public async Task OnBeforeCommitAsync()
        {
            InjectClassIds();
            await PushAggregateEventsAsync();
            RemoveDeletedEntities();
            CommitAggregates();
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
