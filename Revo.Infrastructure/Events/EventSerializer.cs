using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events
{
    public class EventSerializer : IEventSerializer
    {
        private readonly JsonSerializerOptions jsonSerializerSettings = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        private readonly IVersionedTypeRegistry versionedTypeRegistry;

        public EventSerializer(IVersionedTypeRegistry versionedTypeRegistry,
            Func<JsonSerializerOptions, JsonSerializerOptions> customizeEventJsonSerializer)
        {
            this.versionedTypeRegistry = versionedTypeRegistry;

            jsonSerializerSettings.Converters.Add(new JsonStringEnumConverter());
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

                return (IEvent)JsonSerializer.Deserialize(eventJson, versionedType.ClrType, jsonSerializerSettings);
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

            return (JsonSerializer.Serialize(@event, @event.GetType(), jsonSerializerSettings), versionedType.Id);
        }

        public string SerializeEventMetadata(IReadOnlyDictionary<string, string> metadata)
        {
            return JsonSerializer.Serialize(metadata);
        }

        public IReadOnlyDictionary<string, string> DeserializeEventMetadata(string metadataJson)
        {
            return new JsonMetadata(string.IsNullOrWhiteSpace(metadataJson)
                ? [] : JsonObject.Parse(metadataJson).AsObject());
        }
    }
}
