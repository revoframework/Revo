using System;
using System.Collections;
using System.Collections.Generic;

namespace Revo.Core.Collections
{
    public class DoubleDictionary<K1, K2, V> : IDoubleDictionary<K1, K2, V>
    {
        private IDictionary<K1, IDictionary<K2, V>> internalDictionary;

        public DoubleDictionary()
        {
            internalDictionary = new Dictionary<K1, IDictionary<K2, V>>();
        }        

        public IDictionary<K2, V> this[K1 key]
        {
            get
            {
                return internalDictionary[key];
            }
            set
            {
                if (internalDictionary.ContainsKey(key))
                    internalDictionary[key] = value;
                else
                    internalDictionary.Add(key,value);
            }
        }

        public V this[K1 key1, K2 key2]
        {
            get
            {
                return internalDictionary[key1][key2];
            }
            set
            {
                if (!internalDictionary.ContainsKey(key1))
                    internalDictionary.Add(key1, new Dictionary<K2, V>() { { key2, value } });
                else
                    if (internalDictionary[key1] == null)
                    internalDictionary[key1] = new Dictionary<K2, V>() { { key2, value } };
                else
                        if (!internalDictionary[key1].ContainsKey(key2))
                    internalDictionary[key1].Add( key2, value);
                else
                    internalDictionary[key1][key2] = value;
            }
        }

        public void Add(K1 key1, K2 key2, V value)
        {
            if (internalDictionary.ContainsKey(key1)
                && internalDictionary[key1] != null
                && internalDictionary[key1].ContainsKey(key2))
                throw new InvalidOperationException($"There is item with equal keys {key1} and {key2}. Cannot add item with duplicate keys.");
            this[key1, key2] = value;
        }

        public void AddOrSet(K1 key1, K2 key2, V value)
        {
            this[key1, key2] = value;
        }

        public bool ContainsKeys(K1 key)
        {
            return internalDictionary.ContainsKey(key);
        }

        public bool ContainsKey(K1 key1, K2 key2)
        {
            return ContainsKeys(key1) && internalDictionary[key1] != null && internalDictionary[key1].ContainsKey(key2);
        }

        public void DeleteKey(K1 key1, K2 key2)
        {
            if (!ContainsKey(key1, key2))
                throw new InvalidOperationException($"There is no any item with keys {key1} and {key2}");
            internalDictionary[key1].Remove(key2);
        }

        public void DeleteKeys(K1 key1)
        {
            if (!ContainsKeys(key1))
                throw new InvalidOperationException($"There are no any keys associated with key {key1}");
            internalDictionary.Remove(key1);
        }

        public IEnumerator GetEnumerator()
        {
            return internalDictionary.GetEnumerator();
        }
    }
}
