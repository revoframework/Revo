using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Revo.Core.ValueObjects
{
    public class ListAsValue<T> : ValueObject<ListAsValue<T>>, IEnumerable<T>
    {
        public ListAsValue(ImmutableArray<T> list)
        {
            List = list;
        }

        public ListAsValue(IImmutableList<T> list)
        {
            List = list;
        }

        public IReadOnlyList<T> List { get; }

        public IEnumerator<T> GetEnumerator() => List.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)List).GetEnumerator();

        protected override int CalculateHashCode()
        {
            int newHashCode = 0;
            for (int i = 0; i < List.Count; i++)
            {
                newHashCode = (newHashCode * 397) ^ (List[i]?.GetHashCode() ?? 0);
            }

            return newHashCode;
        }

        protected override bool EqualsInternal(ListAsValue<T> other)
        {
            var y = other.List;

            if (List.Count != y.Count)
            {
                return false;
            }

            for (int i = 0; i < List.Count; i++)
            {
                if (!Equals(List[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        protected override IEnumerable<(string Name, object Value)> GetValueComponents()
        {
            return List.Select((x, i) => (i.ToString(), (object)x));
        }
    }
}
