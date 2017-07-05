using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
