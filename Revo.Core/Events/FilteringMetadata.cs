using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Events
{
    public class FilteringMetadata : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> metadata;
        private readonly string[] removedKeys;

        public FilteringMetadata(IReadOnlyDictionary<string, string> metadata,
            params string[] removedKeys)
        {
            this.metadata = metadata;
            this.removedKeys = removedKeys;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var pair in metadata)
            {
                if (!removedKeys.Contains(pair.Key))
                {
                    yield return pair;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => metadata.Count - removedKeys.Length;

        public bool ContainsKey(string key)
        {
            return !removedKeys.Contains(key)
                && metadata.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (removedKeys.Contains(key))
            {
                value = null;
                return false;
            }

            return metadata.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get
            {
                if (removedKeys.Contains(key))
                {
                    throw new KeyNotFoundException($"Metadata key {key} not found");
                }

                return metadata[key];
            }
        }

        public IEnumerable<string> Keys => metadata.Keys.Except(removedKeys);
        public IEnumerable<string> Values => metadata
            .Where(x => !removedKeys.Contains(x.Key)).Select(x => x.Value);
    }
}