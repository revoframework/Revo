using System.Linq;
using Microsoft.EntityFrameworkCore;
using Revo.Core.Lifecycle;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Entities
{
    public class DbContextFactory : IDbContextFactory, IApplicationStartListener
    {
        private readonly ModelDefinitionDiscovery modelDefinitionDiscovery;
        private readonly IEFCoreConfigurer[] configurers;
        private readonly IEFCoreConvention[] conventions;

        public DbContextFactory(ModelDefinitionDiscovery modelDefinitionDiscovery,
            IEFCoreConfigurer[] configurers,
            IEFCoreConvention[] conventions)
        {
            this.modelDefinitionDiscovery = modelDefinitionDiscovery;
            this.configurers = configurers;
            this.conventions = conventions;
        }

        public DbContext CreateContext(string schemaSpace)
        {
            EnsureLoaded();

            var modelDefinitions = modelDefinitionDiscovery.DiscoverModelDefinitions();

            return new EntityDbContext(
                new DbContextOptions<EntityDbContext>(),
                modelDefinitions.ToArray(),
                configurers,
                conventions);
        }

        public void OnApplicationStarted()
        {
            EnsureLoaded();
        }
        private void EnsureLoaded()
        {
        }
    }
}
