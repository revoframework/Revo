using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreEventSourcedAggregateStore : EventSourcedAggregateStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventSourcedAggregateStore(IEventStore eventStore, IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer, IRepositoryFilter[] repositoryFilters,
            IEventMessageFactory eventMessageFactory, IEventSourcedAggregateFactory eventSourcedAggregateFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(eventStore, entityTypeManager, publishEventBuffer, repositoryFilters, eventMessageFactory,
                eventSourcedAggregateFactory)
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
