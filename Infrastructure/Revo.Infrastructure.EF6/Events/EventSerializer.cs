using System;
using System.Collections.Generic;
using System.IO;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.EF6.Events
{
    public class EventSerializer : IEventSerializer
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None
        };

        private readonly IVersionedTypeRegistry versionedTypeRegistry;

        public EventSerializer(IVersionedTypeRegistry versionedTypeRegistry)
        {
            this.versionedTypeRegistry = versionedTypeRegistry;
        }

        public IEvent DeserializeEvent(string eventJson, VersionedTypeId typeId)
        {
            try
            {
                JObject eventObject = JObject.Parse(eventJson);
                Type clrType = versionedTypeRegistry.GetTypeInfo<IEvent>(typeId).ClrType;

                return (IEvent)eventObject.ToObject(clrType);
            }
            catch (JsonException e)
            {
                throw new IOException($"Invalid event read from EF6 event store, error deserializing event JSON: {e.ToString()}", e);
            }
        }

        public (string EventJson, VersionedTypeId TypeId) SerializeEvent(IEvent @event)
        {
            return (JsonConvert.SerializeObject(@event, JsonSerializerSettings),
                versionedTypeRegistry.GetTypeInfo<IEvent>(@event.GetType()).Id);
        }

        public string SerializeEventMetadata(IReadOnlyDictionary<string, string> metadata)
        {
            JObject json = new JObject();
            metadata.ForEach(x => json[x.Key] = x.Value);
            return json.ToString(Formatting.None);
        }

        public JsonMetadata DeserializeEventMetadata(string metadataJson)
        {
            return new JsonMetadata(JObject.Parse(metadataJson));
        }
    }
}
