using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Revo.Core.Events
{
    public class JsonMetadata : IReadOnlyDictionary<string, string>
    {
        private readonly JObject jsonMetadata;

        public JsonMetadata(JObject jsonMetadata)
        {
            this.jsonMetadata = jsonMetadata;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var pair in jsonMetadata)
            {
                yield return new KeyValuePair<string, string>(pair.Key, pair.Value.ToString());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => jsonMetadata.Count;
        public bool ContainsKey(string key)
        {
            return jsonMetadata[key] != null;
        }

        public bool TryGetValue(string key, out string value)
        {
            if (jsonMetadata.TryGetValue(key, out JToken token))
            {
                value = token.Type != JTokenType.Null ? token.ToString() : null;
                return true;
            }

            value = null;
            return false;
        }

        public string this[string key] => jsonMetadata[key]?.ToString() ?? throw new ArgumentException($"JSON metadata key not found: {key}");

        public IEnumerable<string> Keys => jsonMetadata.Properties().Select(x => x.Name);
        public IEnumerable<string> Values => jsonMetadata.PropertyValues().Select(x => x.ToString());
    }
}
