using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Revo.Infrastructure.DataAccess
{
    public class DatabaseInitializerDependencySorter : IDatabaseInitializerSorter
    {
        public List<IDatabaseInitializer> GetSorted(IReadOnlyCollection<IDatabaseInitializer> initializers)
        {
            var result = new List<IDatabaseInitializer>();
            var remaining = initializers.ToList();

            while (remaining.Count > 0)
            {
                var next = remaining
                    .Where(x => GetDependencies(x)
                        .All(depType => result.Any(depType.IsInstanceOfType)))
                    .ToArray();
                if (next.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"Unable to sort IDatabaseInitializers - either you have cyclic dependencies or some dependencies were not found: ${string.Join(", ", remaining.Select(x => x.GetType().Name))}");
                }

                result.AddRange(next);
                foreach (var di in next)
                {
                    remaining.Remove(di);
                }
            }

            return result;
        }

        private IEnumerable<Type> GetDependencies(IDatabaseInitializer databaseInitializer)
        {
            return databaseInitializer.GetType()
                .GetCustomAttributes<InitializeAfterAttribute>()
                .Select(x => x.InitializerType);
        }
    }
}
