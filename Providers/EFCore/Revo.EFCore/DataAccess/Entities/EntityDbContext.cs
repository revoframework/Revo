using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EntityDbContext : DbContext
    {
        private readonly IEFCoreModelDefinition[] modelDefinitions;
        private readonly IEFCoreConvention[] conventions;

        public EntityDbContext(DbContextOptions<EntityDbContext> options,
            IEnumerable<IEFCoreModelDefinition> modelDefinitions,
            IEnumerable<IEFCoreConvention> conventions)
            : base(options)
        {
            this.modelDefinitions = modelDefinitions.ToArray();
            this.conventions = conventions.ToArray();
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var convention in conventions)
            {
                convention.Initialize(modelBuilder);
            }

            foreach (var modelDefinition in modelDefinitions)
            {
                modelDefinition.OnModelCreating(modelBuilder);
            }

            foreach (var convention in conventions)
            {
                convention.Finalize(modelBuilder);
            }
        }
    }
}
