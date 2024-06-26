using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Revo.Core.ValueObjects
{
    public class SetAsValue<T>(IImmutableSet<T> set) : ValueObject<SetAsValue<T>>, IEnumerable<T>
    {
        public IReadOnlyCollection<T> Set { get; } = set;

        public IEnumerator<T> GetEnumerator() => Set.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Set).GetEnumerator();

        protected override int CalculateHashCode()
        {
            int[] componentHashCodes = new int[Set.Count];

            int i = 0;
            foreach (T element in Set)
            {
                componentHashCodes[i] = element?.GetHashCode() ?? 0;
                i++;
            }

            Array.Sort(componentHashCodes);
            int newHashCode = 0;
            for (i = 0; i < componentHashCodes.Length; i++)
            {
                newHashCode = (newHashCode * 397) ^ componentHashCodes[i];
            }

            return newHashCode;
        }

        protected override bool EqualsInternal(SetAsValue<T> other)
        {
            var y = other.Set;

            if (Set.Count != y.Count)
            {
                return false;
            }

            foreach (T element in Set)
            {
                if (!y.Contains(element))
                {
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents() =>
            Set.Select(x => ((string)null, (object)x));
    }
}
