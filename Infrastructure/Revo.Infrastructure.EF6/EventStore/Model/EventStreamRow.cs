using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.EventStore.Model
{
    [TablePrefix(NamespacePrefix = "RES", ColumnPrefix = "ESR")]
    [DatabaseEntity]
    public class EventStreamRow
    {
        public EventStreamRow(Guid id, string eventJson, string eventName, int eventVersion,
            Guid streamId, long streamSequenceNumber,
            DateTimeOffset storeDate, string additionalMetadataJson)
        {
            EventJson = eventJson;
            EventName = eventName;
            EventVersion = eventVersion;
            Id = id;
            StreamId = streamId;
            StreamSequenceNumber = streamSequenceNumber;
            StoreDate = storeDate;
            AdditionalMetadataJson = additionalMetadataJson;
            GlobalSequenceNumber = -1;
        }

        protected EventStreamRow()
        {
        }
        
        public Guid Id { get; private set; }
        public Guid StreamId { get; private set; }
        public DateTimeOffset StoreDate { get; private set; }
        public string EventName { get; private set; }
        public int EventVersion { get; private set; }
        public long StreamSequenceNumber { get; private set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GlobalSequenceNumber { get; private set; }

        public string EventJson { get; private set; }
        public string AdditionalMetadataJson { get; private set; }
        public bool IsDispatchedToAsyncQueues { get; private set; }
        
        public void MarkDispatchedToAsyncQueues()
        {
            IsDispatchedToAsyncQueues = true;
        }
    }
}
