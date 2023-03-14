using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Revo.Core.Types;

namespace Revo.Infrastructure.DataAccess
{
    public class DatabaseInitializerDiscovery : IDatabaseInitializerDiscovery
    {
        private readonly ITypeExplorer typeExplorer;
        private readonly StandardKernel kernel;

        public DatabaseInitializerDiscovery(ITypeExplorer typeExplorer, StandardKernel kernel)
        {
            this.typeExplorer = typeExplorer;
            this.kernel = kernel;
        }

        public IEnumerable<IDatabaseInitializer> DiscoverDatabaseInitializers()
        {
            var databaseInitializerTypes = typeExplorer.GetAllTypes()
                .Where(x => typeof(IDatabaseInitializer).IsAssignableFrom(x)
                            && x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition);

            RegisterDatabaseInitializers(databaseInitializerTypes);
            return GetDatabaseInitializers();
        }

        private void RegisterDatabaseInitializers(IEnumerable<Type> databaseInitializerTypes)
        {
            var availableDatabaseInitializers = GetDatabaseInitializers();

            foreach (Type databaseInitializerType in databaseInitializerTypes)
            {
                if (!availableDatabaseInitializers.Any(x => x.GetType() == databaseInitializerType))
                {
                    kernel.Bind<IDatabaseInitializer>()
                        .To(databaseInitializerType)
                        .InSingletonScope();
                }
            }
        }

        private List<IDatabaseInitializer> GetDatabaseInitializers()
        {
            return kernel.GetAll<IDatabaseInitializer>().ToList();
        }
    }
}
