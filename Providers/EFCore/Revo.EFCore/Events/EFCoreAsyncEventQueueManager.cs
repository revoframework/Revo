using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.EFCore.UnitOfWork;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.EFCore.Events
{
    public class EFCoreAsyncEventQueueManager : AsyncEventQueueManager, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;
        private readonly List<QueuedAsyncEvent> transactionQueuedEvents = new List<QueuedAsyncEvent>();

        public EFCoreAsyncEventQueueManager(ICrudRepository crudRepository, IEventSerializer eventSerializer,
            IExternalEventStore externalEventStore,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, eventSerializer, externalEventStore)
        {
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }
        
        public override async Task<IReadOnlyCollection<IAsyncEventQueueRecord>> CommitAsync()
        {
            var queuedEvents = await EnqueueEventsToRepositoryAsync();
            transactionQueuedEvents.AddRange(queuedEvents);

            await transactionCoordinator.CommitAsync();
            return transactionQueuedEvents.Select(SelectRecordFromQueuedEvent).ToList();
        }

        public async Task OnBeforeCommitAsync()
        {
            var queuedEvents = await EnqueueEventsToRepositoryAsync();
            transactionQueuedEvents.AddRange(queuedEvents);
        }

        public Task OnCommitSucceededAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
