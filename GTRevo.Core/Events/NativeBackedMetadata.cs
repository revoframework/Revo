using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GTRevo.Core.Events
{
    public class NativeBackedMetadata : IReadOnlyDictionary<string, string>
    {
        private readonly IReadOnlyDictionary<string, string> values;
        private readonly IReadOnlyDictionary<string, Func<string>> getters;

        public NativeBackedMetadata(IReadOnlyDictionary<string, string> values, IReadOnlyDictionary<string, Func<string>> getters)
        {
            this.values = values;
            this.getters = getters;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (KeyValuePair<string, Func<string>> getter in getters)
            {
                yield return new KeyValuePair<string, string>(getter.Key, getter.Value());
            }

            foreach (var pair in values)
            {
                yield return pair;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => values.Count + getters.Count;

        public string this[string key] => getters.TryGetValue(key, out var getter) ? getter() : values[key];

        public IEnumerable<string> Keys => getters.Keys.Concat(values.Keys);
        public IEnumerable<string> Values => getters.Values.Select(x => x()).Concat(values.Values);
        
        public bool ContainsKey(string key)
        {
            return getters.ContainsKey(key) || values.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            if (getters.TryGetValue(key, out var getter))
            {
                value = getter();
                return true;
            }

            if (values.TryGetValue(key, out var val))
            {
                value = val;
                return true;
            }

            value = null;
            return false;
        }
    }
}
