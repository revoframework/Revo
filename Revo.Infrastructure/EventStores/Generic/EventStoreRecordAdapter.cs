using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class EventStoreRecordAdapter(EventStreamRow row, IEventSerializer eventSerializer)
        : IEventStreamRowEventStoreRecord
    {
        private IEvent @event;
        private IReadOnlyDictionary<string, string> additionalMetadata;

        public IEvent Event => @event ??= eventSerializer.DeserializeEvent(row.EventJson,
            new VersionedTypeId(row.EventName, row.EventVersion));

        public IReadOnlyDictionary<string, string> AdditionalMetadata => additionalMetadata ??= eventSerializer.DeserializeEventMetadata(row.AdditionalMetadataJson);

        public Guid EventId => row.Id;
        public long StreamSequenceNumber => row.StreamSequenceNumber;
        public DateTimeOffset StoreDate => row.StoreDate;
        public EventStreamRow EventStreamRow => row;
    }
}
