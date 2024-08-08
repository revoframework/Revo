using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Revo.Core.Events
{
    public class JsonMetadata : IReadOnlyDictionary<string, string>
    {
        private readonly JsonObject jsonMetadata;

        public JsonMetadata(JsonObject jsonMetadata)
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
            if (jsonMetadata.TryGetPropertyValue(key, out JsonNode token))
            {
                value = token?.ToString();
                return true;
            }

            value = null;
            return false;
        }

        public string this[string key] => jsonMetadata[key]?.ToString() ?? throw new ArgumentException($"JSON metadata key not found: {key}");

        public IEnumerable<string> Keys => jsonMetadata.Select(x => x.Key);
        public IEnumerable<string> Values => jsonMetadata.Select(x => x.Value.ToString());
    }
}
