using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Revo.DataAccess.Entities;
using Revo.EFCore.UnitOfWork;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.EFCore.EventStores
{
    public class EFCoreExternalEventStore : ExternalEventStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;
        private readonly HashSet<ExternalEventRecord> transactionRecords = new HashSet<ExternalEventRecord>();

        public EFCoreExternalEventStore(ICrudRepository crudRepository,
            IEventSerializer eventSerializer, IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, eventSerializer)
        {
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override async Task<ExternalEventRecord[]> CommitAsync()
        {
            var records = await PrepareRecordsAsync();
            records.ForEach(x => transactionRecords.Add(x));

            await transactionCoordinator.CommitAsync();
            return transactionRecords.ToArray();
        }

        public async Task OnBeforeCommitAsync()
        {
            var records = await PrepareRecordsAsync();
            records.ForEach(x => transactionRecords.Add(x));
        }

        public Task OnCommitSucceededAsync()
        {
            transactionRecords.Clear();
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            transactionRecords.Clear();
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
