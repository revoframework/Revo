using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Extensions.History.ChangeTracking.Model
{
    [TablePrefix(NamespacePrefix = "RHI", ColumnPrefix = "EAD")]
    public class EntityAttributeData : EntityReadModel
    {
        private string attributeValueMapJson;
        private JObject attributeValueMap;

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
            get { return attributeValueMapJson ?? (attributeValueMapJson = attributeValueMap?.ToString(Formatting.None) ?? "{}"); }

            private set
            {
                attributeValueMapJson = value;
                attributeValueMap = JObject.Parse(attributeValueMapJson);
            }
        }

        public bool TryGetAttributeValue<T>(string attributeName, out T value)
        {
            JToken token = null;
            if (attributeValueMap?.TryGetValue(attributeName, out token) ?? false)
            {
                value = (T)(dynamic)token;
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }

        public void SetAttributeValue<T>(string attributeName, T attributeValue)
        {
            if (attributeValueMap == null)
            {
                attributeValueMap = new JObject();
            }

            attributeValueMap[attributeName] = attributeValue != null ? JToken.FromObject(attributeValue) : JValue.CreateNull();
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
