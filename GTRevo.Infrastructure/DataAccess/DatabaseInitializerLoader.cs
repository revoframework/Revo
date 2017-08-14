using GTRevo.Core.Core.Lifecycle;

namespace GTRevo.Infrastructure.DataAccess
{
    public class DatabaseInitializerLoader : IApplicationStartListener
    {
        private readonly IDatabaseInitializerDiscovery databaseInitializerDiscovery;

        public DatabaseInitializerLoader(IDatabaseInitializerDiscovery databaseInitializerDiscovery)
        {
            this.databaseInitializerDiscovery = databaseInitializerDiscovery;
        }

        public void OnApplicationStarted()
        {
            var initializers = databaseInitializerDiscovery.DiscoverDatabaseInitializers();
            foreach (IDatabaseInitializer initializer in initializers)
            {
                initializer.Initialize();
            }
        }
    }
}
