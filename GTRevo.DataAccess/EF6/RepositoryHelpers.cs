using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.DataAccess.EF6
{
    public static class RepositoryHelpers
    {
        public static void ThrowIfGetFailed<T>(T t, params object[] id)
        {
            if (t == null)
            {
                throw new ArgumentException($"{typeof(T).FullName} with ID '{string.Join(", ", id)}' was not found");
            }
        }
    }
}
