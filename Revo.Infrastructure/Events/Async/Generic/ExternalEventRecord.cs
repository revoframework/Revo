using System;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Events.Async.Generic
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "EER")]
    [DatabaseEntity]
    public class ExternalEventRecord
    {
        public ExternalEventRecord(Guid id, string eventJson, string eventName, int eventVersion,
            string metadataJson)
        {
            Id = id;
            EventJson = eventJson;
            EventName = eventName;
            EventVersion = eventVersion;
            MetadataJson = metadataJson;
        }

        protected ExternalEventRecord()
        { 
        }

        public Guid Id { get; private set; }
        public string EventName { get; private set; }
        public int EventVersion { get; private set; }
        public string EventJson { get; private set; }
        public string MetadataJson { get; private set; }
    }
}
