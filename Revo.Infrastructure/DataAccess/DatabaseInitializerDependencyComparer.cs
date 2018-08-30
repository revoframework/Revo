using System.Linq;
using System.Reflection;

namespace Revo.Infrastructure.DataAccess
{
    public class DatabaseInitializerDependencyComparer: IDatabaseInitializerComparer
    {
        public int Compare(IDatabaseInitializer x, IDatabaseInitializer y)
        {
            if (y.GetType().GetCustomAttributes<InitializeAfterAttribute>().Any(a => a.InitializerType == x.GetType()))
                return -1;
            if (x.GetType().GetCustomAttributes<InitializeAfterAttribute>().Any(a => a.InitializerType == y.GetType()))
                return 1;
            return 0;
        }
    }
}
