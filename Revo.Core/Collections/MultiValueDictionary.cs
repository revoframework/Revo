using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Collections
{
    public class MultiValueDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyCollection<TValue>>
    {
        private readonly Dictionary<TKey, List<TValue>> dictionary;
        public MultiValueDictionary()
        {
            dictionary = new Dictionary<TKey, List<TValue>>();
        }

        public MultiValueDictionary(IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, List<TValue>>(comparer);
        }

        public MultiValueDictionary(int capacity)
        {
            dictionary = new Dictionary<TKey, List<TValue>>(capacity);
        }

        public MultiValueDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            dictionary = new Dictionary<TKey, List<TValue>>(capacity, comparer);
        }

        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable) : this()
        {
            AddRange(enumerable);
        }

        public MultiValueDictionary(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> enumerable,
            IEqualityComparer<TKey> comparer) : this(comparer)
        {
            AddRange(enumerable);
        }

        public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> values) : this()
        {
            AddRange(values);
        }

        public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> values,
            IEqualityComparer<TKey> comparer) : this(comparer)
        {
            AddRange(values);
        }

        public IReadOnlyCollection<TValue> this[TKey key]
        {
            get
            {
                if (dictionary.TryGetValue(key, out var listValue))
                {
                    return listValue;
                }

                throw new KeyNotFoundException();
            }
        }

        public int Count => dictionary.Count;
        public IEnumerable<TKey> Keys => dictionary.Keys;
        public IEnumerable<IReadOnlyCollection<TValue>> Values => dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            if (!dictionary.TryGetValue(key, out var values))
            {
                values = dictionary[key] = new List<TValue>();
            }

            values.Add(value);
        }

        public void AddRange(TKey key, IEnumerable<TValue> values)
        {
            if (!dictionary.TryGetValue(key, out var list))
            {
                list = dictionary[key] = new List<TValue>();
            }

            list.AddRange(values);
        }

        public void AddRange(IEnumerable<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> values)
        {
            foreach (var pair in values)
            {
                if (!dictionary.TryGetValue(pair.Key, out var list))
                {
                    list = dictionary[pair.Key] = new List<TValue>();
                }

                list.AddRange(pair.Value);
            }
        }

        public void AddRange(IEnumerable<IGrouping<TKey, TValue>> values)
        {
            foreach (var pair in values)
            {
                if (!dictionary.TryGetValue(pair.Key, out var list))
                {
                    list = dictionary[pair.Key] = new List<TValue>();
                }

                list.AddRange(pair);
            }
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>> GetEnumerator()
        {
            return new Enumerator(dictionary.GetEnumerator());
        }
        
        public bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> value)
        {
            bool result = dictionary.TryGetValue(key, out var listValue);
            value = listValue;
            return result;
        }

        public bool Remove(TKey key)
        {
            return dictionary.Remove(key);
        }

        public bool Remove(TKey key, TValue value)
        {
            if (dictionary.TryGetValue(key, out var values) && values.Remove(value))
            {
                if (values.Count == 0)
                {
                    dictionary.Remove(key);
                }

                return true;
            }

            return false;
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Enumerator : IEnumerator<KeyValuePair<TKey, IReadOnlyCollection<TValue>>>
        {
            private readonly IEnumerator<KeyValuePair<TKey, List<TValue>>> enumerator;

            public Enumerator(IEnumerator<KeyValuePair<TKey, List<TValue>>> enumerator)
            {
                this.enumerator = enumerator;
            }

            public bool MoveNext()
            {
                return enumerator.MoveNext();
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            public KeyValuePair<TKey, IReadOnlyCollection<TValue>> Current =>
                new KeyValuePair<TKey, IReadOnlyCollection<TValue>>(
                    enumerator.Current.Key,
                    enumerator.Current.Value);

            object IEnumerator.Current => ((IEnumerator)enumerator).Current;

            public void Dispose()
            {
                enumerator.Dispose();
            }
        }
    }
}
