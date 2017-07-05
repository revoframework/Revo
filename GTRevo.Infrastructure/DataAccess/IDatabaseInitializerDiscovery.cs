using System.Collections.Generic;

namespace GTRevo.Infrastructure.DataAccess
{
    public interface IDatabaseInitializerDiscovery
    {
        IEnumerable<IDatabaseInitializer> DiscoverDatabaseInitializers();
    }
}