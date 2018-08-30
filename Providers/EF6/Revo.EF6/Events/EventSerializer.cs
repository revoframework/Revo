using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.EF6.Events
{
    public class EventSerializer : IEventSerializer
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            },
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
                Type clrType = versionedTypeRegistry.GetTypeInfo<IEvent>(typeId).ClrType;
                return (IEvent)JsonConvert.DeserializeObject(eventJson, clrType);
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
            return JsonConvert.SerializeObject(metadata, Formatting.None);
        }

        public JsonMetadata DeserializeEventMetadata(string metadataJson)
        {
            return new JsonMetadata(metadataJson?.Length > 0
                ? JObject.Parse(metadataJson) : new JObject());
        }
    }
}
