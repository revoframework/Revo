using System.Collections.Generic;
using System.Collections.Immutable;

namespace Revo.Core.ValueObjects
{
    public static class CollectionAsValueExtensions
    {
        /// <summary>
        /// Wraps an immutable list for use in ValueObject&lt;T&gt;.GetValueComponents
        /// with a decorator that correctly implements value object semantics for lists.
        /// </summary>
        public static IEnumerable<T> AsValueObject<T>(this IImmutableList<T> list)
        {
            return new ListAsValue<T>(list);
        }

        /// <summary>
        /// Wraps an immutable array for use in ValueObject&lt;T&gt;.GetValueComponents
        /// with a decorator that correctly implements value object semantics for arrays.
        /// </summary>
        public static IEnumerable<T> AsValueObject<T>(this ImmutableArray<T> array)
        {
            return new ListAsValue<T>(array);
        }

        /// <summary>
        /// Wraps an immutable set for use in ValueObject&lt;T&gt;.GetValueComponents
        /// with a decorator that correctly implements value object semantics for sets.
        /// </summary>
        public static IEnumerable<T> AsValueObject<T>(this IImmutableSet<T> set)
        {
            return new SetAsValue<T>(set);
        }

        /// <summary>
        /// Wraps an immutable dictionary for use in ValueObject&lt;T&gt;.GetValueComponents
        /// with a decorator that correctly implements value object semantics for dictionaries.
        /// </summary>
        public static IReadOnlyDictionary<TKey, TValue> AsValueObject<TKey, TValue>(this IImmutableDictionary<TKey, TValue> dictionary)
        {
            return new DictionaryAsValue<TKey, TValue>(dictionary);
        }
    }
}
