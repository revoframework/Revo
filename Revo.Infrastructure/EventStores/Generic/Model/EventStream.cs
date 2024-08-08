using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using Revo.Core.Events;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.EventStores.Generic.Model
{
    [TablePrefix(NamespacePrefix = "RES", ColumnPrefix = "EVS")]
    [DatabaseEntity]
    public class EventStream : IRowVersioned, IHasId<Guid>
    {
        private IReadOnlyDictionary<string, string> metadata;
        private JsonObject metadataJsonObject;

        public EventStream(Guid id)
        {
            Id = id;
        }

        protected EventStream()
        {
        }

        public Guid Id { get; private set; }
        public string MetadataJson { get; private set; }
        public int Version { get; set; }

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
                                ? JsonObject.Parse(MetadataJson).AsObject()
                                : [];
                        }
                        catch (JsonException e)
                        {
                            throw new IOException($"Invalid {GetType().Name} read, error deserializing JSON data: {e.ToString()}", e);
                        }
                    }

                    metadata = new JsonMetadata(metadataJsonObject);
                }

                return metadata;
            }

            set
            {
                metadataJsonObject = [];
                foreach (var pair in value)
                {
                    metadataJsonObject[pair.Key] = pair.Value;
                }

                MetadataJson = metadataJsonObject.ToString();
                metadata = new JsonMetadata(metadataJsonObject);
            }
        }
    }
}
