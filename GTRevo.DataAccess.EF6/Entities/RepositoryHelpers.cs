using System;
using GTRevo.DataAccess.Entities;

namespace GTRevo.DataAccess.EF6.Entities
{
    public static class RepositoryHelpers
    {
        public static void ThrowIfGetFailed<T>(T t, params object[] id)
        {
            if (t == null)
            {
                throw new EntityNotFoundException($"{typeof(T).FullName} with ID '{string.Join(", ", id)}' was not found");
            }
        }
    }
}
