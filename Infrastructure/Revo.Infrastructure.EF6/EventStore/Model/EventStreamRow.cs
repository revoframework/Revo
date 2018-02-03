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
    public class EventStreamRow : IEventStoreRecord
    {
        private static readonly string[] FilteredMetadataKeys =
        {
            BasicEventMetadataNames.EventId,
            BasicEventMetadataNames.StreamSequenceNumber,
            BasicEventMetadataNames.StoreDate,
            BasicEventMetadataNames.PublishDate,
        };

        private IEvent @event;
        private IReadOnlyDictionary<string, string> additionalMetadata;
        private JObject additionalMetadataJsonObject;
        
        public EventStreamRow(IDomainEventTypeCache domainEventTypeCache, Guid id,
            IEvent @event, Guid streamId, long streamSequenceNumber, DateTimeOffset storeDate,
            IReadOnlyDictionary<string, string> additionalMetadata)
        {
            DomainEventTypeCache = domainEventTypeCache;
            Event = @event;
            AdditionalMetadata = additionalMetadata;
            Id = id;
            StreamId = streamId;
            StreamSequenceNumber = streamSequenceNumber;
            StoreDate = storeDate;
            GlobalSequenceNumber = -1;
        }

        protected EventStreamRow()
        {
        }

        [NotMapped]
        public IDomainEventTypeCache DomainEventTypeCache { get; set; }

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
                        throw new IOException($"Invalid event read from EF6EventStore, error deserializing JSON metadata: {e.ToString()}", e);
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
        public IReadOnlyDictionary<string, string> AdditionalMetadata
        {
            get
            {
                if (additionalMetadata == null)
                {
                    if (additionalMetadataJsonObject == null)
                    {
                        try
                        {
                            additionalMetadataJsonObject = AdditionalMetadataJson?.Length > 0
                                ? JObject.Parse(AdditionalMetadataJson)
                                : new JObject();
                        }
                        catch (JsonException e)
                        {
                            throw new IOException($"Invalid event read from EF6EventStore, error deserializing JSON data: {e.ToString()}", e);
                        }
                    }

                    additionalMetadata = new JsonMetadata(additionalMetadataJsonObject);
                }

                return additionalMetadata;
            }

            set
            {
                additionalMetadataJsonObject = new JObject();
                foreach (var pair in value)
                {
                    if (FilteredMetadataKeys.Contains(pair.Key))
                    {
                        continue;
                    }

                    additionalMetadataJsonObject[pair.Key] = pair.Value;
                }

                AdditionalMetadataJson = additionalMetadataJsonObject.ToString(Formatting.None);
                additionalMetadata = new JsonMetadata(additionalMetadataJsonObject);
            }
        }
        
        public Guid EventId => Id;

        public void MarkDispatchedToAsyncQueues()
        {
            IsDispatchedToAsyncQueues = true;
        }
    }
}
