using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Revo.Core.ValueObjects
{
    public class DictionaryAsValue<TKey, TValue> : ValueObject<DictionaryAsValue<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
    {
        public DictionaryAsValue(IImmutableDictionary<TKey, TValue> dictionary)
        {
            Dictionary = dictionary;
        }

        public IReadOnlyDictionary<TKey, TValue> Dictionary { get; }
        public int Count => Dictionary.Count;
        public TValue this[TKey key] => Dictionary[key];
        public IEnumerable<TKey> Keys => Dictionary.Keys;
        public IEnumerable<TValue> Values => Dictionary.Values;

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);
        public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

        protected override int CalculateHashCode()
        {
            int[] componentHashCodes = new int[Dictionary.Count * 2];

            int i = 0;
            foreach (var pair in Dictionary)
            {
                componentHashCodes[i] = pair.Key?.GetHashCode() ?? 0;
                componentHashCodes[i + 1] = pair.Value?.GetHashCode() ?? 0;
                i += 2;
            }

            Array.Sort(componentHashCodes);
            int newHashCode = 0;
            for (i = 0; i < componentHashCodes.Length; i++)
            {
                newHashCode = (newHashCode * 397) ^ componentHashCodes[i];
            }

            return newHashCode;
        }

        protected override bool EqualsInternal(DictionaryAsValue<TKey, TValue> other)
        {
            var y = other.Dictionary;

            if (Dictionary.Count != y.Count)
            {
                return false;
            }

            foreach (var xPair in Dictionary)
            {
                if (!y.TryGetValue(xPair.Key, out TValue val) || !Equals(xPair.Value, val))
                {
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            return Dictionary.Select(x => (x.Key.ToString(), (object)x.Value));
        }
    }
}
