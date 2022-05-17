using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess
{
    public interface IDatabaseInitializerSorter
    {
        List<IDatabaseInitializer> GetSorted(IReadOnlyCollection<IDatabaseInitializer> initializers);
    }
}
