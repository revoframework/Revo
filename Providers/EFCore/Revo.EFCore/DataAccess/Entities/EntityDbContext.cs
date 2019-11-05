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
            IEFCoreModelDefinition[] modelDefinitions,
            IEFCoreConvention[] conventions)
            : base(options)
        {
            this.modelDefinitions = modelDefinitions;
            this.conventions = conventions;
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
