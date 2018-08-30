using Microsoft.EntityFrameworkCore;
using MoreLinq;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;
using Revo.EFCore.DataAccess.Model;

namespace Revo.EFCore.DataAccess.Entities
{
    public class EntityDbContext : DbContext
    {
        private readonly IEFCoreModelDefinition[] modelDefinitions;
        private readonly IEFCoreConfigurer[] configurers;
        private readonly IEFCoreConvention[] conventions;

        public EntityDbContext(DbContextOptions<EntityDbContext> options,
            IEFCoreModelDefinition[] modelDefinitions,
            IEFCoreConfigurer[] configurers,
            IEFCoreConvention[] conventions)
            : base(options)
        {
            this.modelDefinitions = modelDefinitions;
            this.configurers = configurers;
            this.conventions = conventions;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            configurers.ForEach(x => x.OnConfiguring(optionsBuilder));
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
