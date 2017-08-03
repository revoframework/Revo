using System.Collections.Generic;
using System.Collections;

namespace GTRevo.Core.Collections
{
    public interface IDoubleDictionary<K1, K2, V>: IEnumerable
    {
        V this[K1 key1, K2 key2] { get; set;}
        IDictionary<K2,V> this[K1 key] { get; set; }
        bool ContainsKey(K1 key1, K2 key2);
        bool ContainsKeys(K1 key);
        void Add(K1 key1, K2 key2, V value);
        void AddOrSet(K1 key1, K2 key2, V value);
        void DeleteKeys(K1 key1);
        void DeleteKey(K1 key1, K2 key2);
    }
}
