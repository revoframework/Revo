using System;
using System.Collections.Generic;
using System.Text.Json;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Extensions.History.ChangeTracking.Model
{
    [TablePrefix(NamespacePrefix = "RHI", ColumnPrefix = "EAD")]
    public class EntityAttributeData : EntityReadModel
    {
        private string attributeValueMapJson;
        private Dictionary<string, dynamic> attributeValueMap = [];

        public EntityAttributeData(Guid id, Guid? aggregateId, Guid? entityId)
        {
            Id = id;
            AggregateId = aggregateId;
            EntityId = entityId;
        }

        protected EntityAttributeData()
        {
        }

        public Guid? AggregateId { get; private set; }
        public Guid? EntityId { get; private set; }

        public string AttributeValueMapJson
        {
            get
            {
                return attributeValueMapJson ??= JsonSerializer.Serialize(attributeValueMap);
            }

            private set
            {
                attributeValueMapJson = value;
                if (string.IsNullOrWhiteSpace(attributeValueMapJson))
                {
                    attributeValueMap = [];
                } 
                else
                {
                    attributeValueMap = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(attributeValueMapJson);
                }
            }
        }

        public bool TryGetAttributeValue<T>(string attributeName, out T value)
        {
            if (attributeValueMap.TryGetValue(attributeName, out dynamic token))
            {
                value = (T)token;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public void SetAttributeValue<T>(string attributeName, T attributeValue)
        {
            attributeValueMap[attributeName] = attributeValue;
            attributeValueMapJson = null;
        }

        /*private IEnumerable<KeyValuePair<string, JToken>> AttributeValues
        {
            get
            {
                return attributeValueMap?.Properties().Select(x => new KeyValuePair<string, JToken>(x.Name, x.Value))
                    ?? Enumerable.Empty<KeyValuePair<string, JToken>>();
            }
        }*/
    }
}
