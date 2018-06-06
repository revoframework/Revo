using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Revo.Core.ValueObjects
{
    /// <summary>
    /// Abstract base class for value objects.
    /// In DDD, value objects have no identity and are only defined by the set of its properties.
    /// Furthermore, their state should typically also be fully immutable.
    /// <para>ValueObject&lt;T&gt; implements basic value semantics, overriding object.Equals,
    /// object.GetHashCode and object.ToString and implementing IEquatable&lt;T&gt;.
    /// To do that, it uses the value components specified by GetValueComponents.</para>
    /// </summary>
    /// <typeparam name="T">Type of the value object class deriving from ValueObject&lt;T&gt; (self-reference).</typeparam>
    public abstract class ValueObject<T> : IEquatable<T>
        where T : ValueObject<T>
    {
        private WeakReference<(string Name, object Value)[]> cachedValueMembers;
        private int? hashCode;

        public ValueObject()
        {
            Debug.Assert(typeof(T).IsAssignableFrom(GetType()));
        }

        /// <summary>
        /// Gets the components that define the value of this objects.
        /// <para>To correctly include collections into the mix, wrap them using the CollectionAsValueExtensions before returning first.
        /// The enumeration itself can easily be implemented using <c>yield return</c> statements and <c>nameof</c> operators.</para>
        /// </summary>
        /// <returns>Value components to be used for equality comparisons, hash code computation and ToString serialization.</returns>
        protected abstract IEnumerable<(string Name, object Value)> GetValueComponents();

        protected (string Name, object Value)[] GetValueComponentsWithCache()
        {
            if (cachedValueMembers == null || !cachedValueMembers.TryGetTarget(out var valueMembers))
            {
                valueMembers = GetValueComponents().ToArray();
                cachedValueMembers = new WeakReference<(string Name, object Value)[]>(valueMembers);
            }

            return valueMembers;
        }

        public bool Equals(T other)
        {
            if (ReferenceEquals(other, this))
            {
                return true;
            }

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return EqualsInternal(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
            {
                return true;
            }

            if (ReferenceEquals(obj, null) || !(obj is T other))
            {
                return false;
            }

            return EqualsInternal(other);
        }

        public override int GetHashCode()
        {
            if (!hashCode.HasValue)
            {
                hashCode = CalculateHashCode();
            }

            return hashCode.Value;
        }

        public override string ToString()
        {
            var valueMembers = GetValueComponentsWithCache();
            return $"{typeof(T).Name} {{ {string.Join(", ", valueMembers.Select(x => (x.Name != null ? x.Name + " = " : "") + (x.Value?.ToString() ?? "null")))} }}";
        }

        protected virtual int CalculateHashCode()
        {
            int newHashCode = 0;

            var components = GetValueComponentsWithCache();
            for (int i = 0; i < components.Length; i++)
            {
                newHashCode = (newHashCode * 397) ^ (components[i].Value?.GetHashCode() ?? 0);
            }

            return newHashCode;
        }

        protected virtual bool EqualsInternal(T other)
        {
            var x = GetValueComponentsWithCache();
            var y = other.GetValueComponentsWithCache();

            if (x.Length != y.Length)
            {
                throw new InvalidOperationException($"Instances of ValueObject<{typeof(T).Name}> did not return same amount of components in GetValueComponents");
            }

            for (int i = 0; i < x.Length; i++)
            {
                if (!Equals(x[i].Value, y[i].Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
