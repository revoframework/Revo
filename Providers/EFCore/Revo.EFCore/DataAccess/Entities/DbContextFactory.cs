using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Entities
{
    public class DbContextFactory : IDbContextFactory
    {
        private readonly ModelDefinitionDiscovery modelDefinitionDiscovery;
        private readonly IEFCoreConfigurer[] configurers;
        private readonly IEFCoreConvention[] conventions;

        public DbContextFactory(ModelDefinitionDiscovery modelDefinitionDiscovery,
            IEnumerable<IEFCoreConfigurer> configurers,
            IEnumerable<IEFCoreConvention> conventions)
        {
            this.modelDefinitionDiscovery = modelDefinitionDiscovery;
            this.configurers = configurers.ToArray();
            this.conventions = conventions.ToArray();

            Array.Sort(this.conventions);
        }

        public DbContext CreateContext(string schemaSpace)
        {
            var modelDefinitions = modelDefinitionDiscovery.DiscoverModelDefinitions();

            var optionsBuilder = new DbContextOptionsBuilder<EntityDbContext>();
            configurers.ForEach(x => x.OnConfiguring(optionsBuilder));
            
            return new EntityDbContext(
                optionsBuilder.Options,
                modelDefinitions.ToArray(),
                conventions);
        }
    }
}
