using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revo.Core.Events;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.EventStores.Generic.Model
{
    [TablePrefix(NamespacePrefix = "RES", ColumnPrefix = "EVS")]
    [DatabaseEntity]
    public class EventStream : IRowVersioned, IHasId<Guid>
    {
        private IReadOnlyDictionary<string, string> metadata;
        private JObject metadataJsonObject;

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
                                ? JObject.Parse(MetadataJson)
                                : new JObject();
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
