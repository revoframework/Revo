using System.Collections.Generic;
using System.Linq;
using Xunit.Sdk;

namespace Revo.Testing.Core
{
    public static class AssertEx
    {
        public static void EqualNoOrder<T>(ICollection<T> expected, ICollection<T> actual)
        {
            if (expected.Count != actual.Count || !expected.All(actual.Contains))
            {
                throw new EqualException(expected, actual);
            }
        }

        public static void EqualNoOrder<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            if (expected.Count() != actual.Count() || !expected.All(actual.Contains))
            {
                throw new EqualException(expected, actual);
            }
        }
    }
}
