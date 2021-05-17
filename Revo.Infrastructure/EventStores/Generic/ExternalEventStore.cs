using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class ExternalEventStore : IExternalEventStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly IEventSerializer eventSerializer;
        private readonly Dictionary<Guid, ExternalEventRecord> externalEvents = new Dictionary<Guid, ExternalEventRecord>();

        public ExternalEventStore(ICrudRepository crudRepository, IEventSerializer eventSerializer)
        {
            this.crudRepository = crudRepository;
            this.eventSerializer = eventSerializer;
        }

        public void TryPushEvent(IEventMessage eventMessage)
        {
            var record = CreateExternalEventRecord(eventMessage);
            if (!externalEvents.ContainsKey(record.Id))
            {
                externalEvents.Add(record.Id, record);
            }
        }

        public async Task<ExternalEventRecord> GetEventAsync(Guid eventId)
        {
            if (externalEvents.TryGetValue(eventId, out var record))
            {
                return record;
            }

            return await crudRepository.GetAsync<ExternalEventRecord>(eventId);
        }

        public virtual async Task<ExternalEventRecord[]> CommitAsync()
        {
            var records = await PrepareRecordsAsync();
            await crudRepository.SaveChangesAsync();
            return records;
        }

        protected virtual async Task<ExternalEventRecord[]> GetExistingEventRecordsAsync(Guid[] ids)
        {
            return await crudRepository
                .FindManyAsync<ExternalEventRecord>(ids);
        }

        protected async Task<ExternalEventRecord[]> PrepareRecordsAsync()
        {
            var externalEventIds = externalEvents.Values.Select(x => x.Id).ToArray();
            var existingRecord = await GetExistingEventRecordsAsync(externalEventIds);

            var records = externalEvents.Values.Select(x =>
            {
                var existing = existingRecord.FirstOrDefault(y => y.Id == x.Id);
                if (existing != null)
                {
                    return existing;
                }

                crudRepository.Add(x);
                return x;
            }).ToArray();

            externalEvents.Clear();

            return records;
        }

        private ExternalEventRecord CreateExternalEventRecord(IEventMessage eventMessage)
        {
            Guid eventId = eventMessage.Metadata.GetEventId() ?? throw new InvalidOperationException($"Cannot store an external event ({eventMessage}) without ID");
            (string eventJson, VersionedTypeId typeId) = eventSerializer.SerializeEvent(eventMessage.Event);
            string metadataJson = eventSerializer.SerializeEventMetadata(
                new FilteringMetadata(eventMessage.Metadata, BasicEventMetadataNames.EventId));

            return new ExternalEventRecord(eventId, eventJson, typeId.Name, typeId.Version, metadataJson);
        }
    }
}
