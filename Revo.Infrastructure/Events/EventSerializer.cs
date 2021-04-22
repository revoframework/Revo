using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events
{
    public class EventSerializer : IEventSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
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

        public EventSerializer(IVersionedTypeRegistry versionedTypeRegistry,
            Func<JsonSerializerSettings, JsonSerializerSettings> customizeEventJsonSerializer)
        {
            this.versionedTypeRegistry = versionedTypeRegistry;

            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
            jsonSerializerSettings = customizeEventJsonSerializer(jsonSerializerSettings);
        }

        public IEvent DeserializeEvent(string eventJson, VersionedTypeId typeId)
        {
            try
            {
                var versionedType = versionedTypeRegistry.GetTypeInfo<IEvent>(typeId);
                if (versionedType == null)
                {
                    throw new ArgumentException($"Cannot deserialize event of an unknown type ID {typeId}");
                }

                Type clrType = versionedType.ClrType;
                return (IEvent)JsonConvert.DeserializeObject(eventJson, clrType, jsonSerializerSettings);
            }
            catch (JsonException e)
            {
                throw new IOException($"Invalid event read from event store, error deserializing event JSON: {e}", e);
            }
        }

        public (string EventJson, VersionedTypeId TypeId) SerializeEvent(IEvent @event)
        {
            var versionedType = versionedTypeRegistry.GetTypeInfo<IEvent>(@event.GetType());
            if (versionedType == null)
            {
                throw new ArgumentException($"Cannot serialize event of an unknown type {@event.GetType()}");
            }

            return (JsonConvert.SerializeObject(@event, jsonSerializerSettings), versionedType.Id);
        }

        public string SerializeEventMetadata(IReadOnlyDictionary<string, string> metadata)
        {
            return JsonConvert.SerializeObject(metadata, Formatting.None);
        }

        public IReadOnlyDictionary<string, string> DeserializeEventMetadata(string metadataJson)
        {
            return new JsonMetadata(metadataJson?.Length > 0
                ? JObject.Parse(metadataJson) : new JObject());
        }
    }
}
