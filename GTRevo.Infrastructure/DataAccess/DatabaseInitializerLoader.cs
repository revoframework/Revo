using System.Linq;
using GTRevo.Core.Core.Lifecycle;

namespace GTRevo.Infrastructure.DataAccess
{
    public class DatabaseInitializerLoader : IApplicationStartListener
    {
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;
        private readonly IDatabaseInitializerComparer comparer;

        public DatabaseInitializerLoader(IDatabaseInitializerDiscovery databaseInitializerDiscovery, IDatabaseInitializerComparer comparer)
        {
            this.databaseInitializerDiscovery = databaseInitializerDiscovery;
            this.comparer = comparer;
        }

        public void OnApplicationStarted()
        {
            var initializers = databaseInitializerDiscovery.DiscoverDatabaseInitializers();
            var sortedInitializers = initializers.ToList();
            sortedInitializers.Sort(comparer);
            for (int i = 0; i< sortedInitializers.Count; i++)
            {
                sortedInitializers[i].Initialize();
            }
        }
    }
}
