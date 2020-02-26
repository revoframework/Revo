using System.Collections.Generic;
using System.Linq;

namespace Revo.DataAccess.Entities
{
    public static class RepositoryHelpers
    {
        public static void ThrowIfGetFailed<T>(T t, params object[] id)
        {
            if (t == null)
            {
                ThrowGetFailed<T>(id);
            }
        }

        public static void ThrowIfGetManyFailed<T, TId>(IReadOnlyCollection<T> ts, params TId[] ids)
            where T : IHasId<TId>
        {
            var missingIds = ids.Where(id => !ts.Any(x => Equals(x.Id, id))).ToArray();
            if (missingIds.Length > 0)
            {
                ThrowGetManyFailed<T, TId>(missingIds);
            }
        }

        public static void ThrowGetFailed<T>(params object[] id)
        {
            throw new EntityNotFoundException($"{typeof(T).FullName} with ID {string.Join(", ", id)} was not found");
        }

        public static void ThrowGetManyFailed<T, TId>(params TId[] ids)
        {
            throw new EntityNotFoundException($"{typeof(T).FullName} with ID(s) {string.Join(", ", ids)} were not found");
        }
    }
}
