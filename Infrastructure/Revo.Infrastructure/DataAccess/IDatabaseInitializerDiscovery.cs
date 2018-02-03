using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess
{
    public interface IDatabaseInitializerDiscovery
    {
        IEnumerable<IDatabaseInitializer> DiscoverDatabaseInitializers();
    }
}