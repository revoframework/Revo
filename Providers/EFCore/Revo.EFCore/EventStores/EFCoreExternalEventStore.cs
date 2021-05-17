using MoreLinq;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.EFCore.Repositories;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EFCore.EventStores
{
    public class EFCoreExternalEventStore : ExternalEventStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;
        private HashSet<ExternalEventRecord> transactionRecords = new HashSet<ExternalEventRecord>();

        public EFCoreExternalEventStore(ICrudRepository crudRepository,
            IEventSerializer eventSerializer, IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, eventSerializer)
        {
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override async Task<ExternalEventRecord[]> CommitAsync()
        {
            var currentTransactionRecords = transactionRecords;
            var records = await PrepareRecordsAsync();
            records.ForEach(x => currentTransactionRecords.Add(x));

            await transactionCoordinator.CommitAsync();
            return currentTransactionRecords.ToArray();
        }

        public async Task OnBeforeCommitAsync()
        {
            var records = await PrepareRecordsAsync();
            records.ForEach(x => transactionRecords.Add(x));
        }

        public Task OnCommitSucceededAsync()
        {
            // new, so we don't clear currentTransactionRecords in CommitAsync
            transactionRecords = new HashSet<ExternalEventRecord>();
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            return Task.CompletedTask;
        }

        protected override async Task<ExternalEventRecord[]> GetExistingEventRecordsAsync(Guid[] ids)
        {
            var records = transactionRecords
                .Where(x => ids.Contains(x.Id))
                .ToArray();

            var missingIds = ids.Where(x => !records.Any(y => y.Id == x)).ToArray();

            if (missingIds.Length > 0)
            {
                records = records
                    .Concat(await base.GetExistingEventRecordsAsync(missingIds))
                    .ToArray();
            }

            return records;
        }
    }
}
