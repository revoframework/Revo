using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Revo.Core.Events;
using Revo.Core.Types;
using Revo.Domain.Events;
using Revo.Infrastructure.EF6.Events;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.EventStore
{
    public class EventStoreRecordAdapter : IEventStoreRecord
    {
        private readonly EventStreamRow row;
        private readonly IEventSerializer eventSerializer;
        private IEvent @event;
        private IReadOnlyDictionary<string, string> additionalMetadata;

        public EventStoreRecordAdapter(EventStreamRow row, IEventSerializer eventSerializer)
        {
            this.row = row;
            this.eventSerializer = eventSerializer;
        }

        public IEvent Event => @event ??
                               (@event = eventSerializer.DeserializeEvent(row.EventJson,
                                   new VersionedTypeId(row.EventName, row.EventVersion)));

        public IReadOnlyDictionary<string, string> AdditionalMetadata => additionalMetadata
                                                                         ?? (additionalMetadata = new JsonMetadata(row.AdditionalMetadataJson?.Length > 0
                                                                                 ? JObject.Parse(row.AdditionalMetadataJson) : new JObject()));

        public Guid EventId => row.Id;
        public long StreamSequenceNumber => row.StreamSequenceNumber;
        public DateTimeOffset StoreDate => row.StoreDate;
        public EventStreamRow EventStreamRow => row;
    }
}
