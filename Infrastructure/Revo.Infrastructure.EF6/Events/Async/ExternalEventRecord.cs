using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;

namespace Revo.Infrastructure.EF6.Events.Async
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "EER")]
    [DatabaseEntity]
    public class ExternalEventRecord
    {
        private IEvent @event;
        private IReadOnlyDictionary<string, string> metadata;
        private JObject metadataJsonObject;

        public ExternalEventRecord(IDomainEventTypeCache domainEventTypeCache,
            IEvent @event, IReadOnlyDictionary<string, string> metadata)
        {
            DomainEventTypeCache = domainEventTypeCache;
            Event = @event;
            Metadata = metadata;
            Id = Metadata.GetEventId() ?? throw new InvalidOperationException($"Cannot async-queue an external async event ({@event.GetType().FullName}) without ID");
        }

        protected ExternalEventRecord()
        { 
        }

        [NotMapped]
        public IDomainEventTypeCache DomainEventTypeCache { get; set; }

        public Guid Id { get; private set; }

        public string EventName { get; private set; }
        public int EventVersion { get; private set; }
        public string EventJson { get; private set; }
        public string MetadataJson { get; private set; }

        [NotMapped]
        public IEvent Event
        {
            get
            {
                if (@event == null)
                {
                    try
                    {
                        JObject eventObject = JObject.Parse(EventJson);
                        Type eventType = DomainEventTypeCache.GetClrEventType(EventName, EventVersion);

                        if (eventType == null)
                        {
                            throw new ArgumentException($"Domain event type not found: {EventName} v. {EventVersion}");
                        }

                        if (!typeof(DomainAggregateEvent).IsAssignableFrom(eventType))
                        {
                            throw new ArgumentException($"Invalid domain aggregate event type: {eventType.FullName}");
                        }

                        @event = (IEvent)eventObject.ToObject(eventType);
                    }
                    catch (JsonException e)
                    {
                        throw new IOException($"Invalid queued async event read from DB, error deserializing JSON metadata: {e.ToString()}", e);
                    }
                }

                return @event;
            }

            private set
            {
                (EventName, EventVersion) = DomainEventTypeCache.GetEventNameAndVersion(value.GetType());
                EventJson = JObject.FromObject(value).ToString(Formatting.None);
                @event = value;
            }
        }

        [NotMapped]
        public IReadOnlyDictionary<string, string> Metadata
        {
            get
            {
                if (metadata == null)
                {
                    if (metadataJsonObject == null)
                    {
                        try
                        {
                            metadataJsonObject = MetadataJson?.Length > 0
                                ? JObject.Parse(MetadataJson)
                                : new JObject();
                        }
                        catch (JsonException e)
                        {
                            throw new IOException($"Invalid queued async event read from DB, error deserializing JSON data: {e.ToString()}", e);
                        }
                    }

                    metadata = new JsonMetadata(metadataJsonObject);
                }

                return metadata;
            }

            set
            {
                metadataJsonObject = new JObject();
                foreach (var pair in value)
                {
                    metadataJsonObject[pair.Key] = pair.Value;
                }

                MetadataJson = metadataJsonObject.ToString(Formatting.None);
                metadata = new JsonMetadata(metadataJsonObject);
            }
        }
    }
}
