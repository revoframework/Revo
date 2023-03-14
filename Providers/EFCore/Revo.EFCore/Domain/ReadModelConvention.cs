using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Revo.Domain.ReadModel;
using Revo.EFCore.DataAccess.Configuration;
using Revo.EFCore.DataAccess.Conventions;

namespace Revo.EFCore.Domain
{
    public class ReadModelConvention : EFCoreConventionBase
    {
        private readonly EFCoreDataAccessConfigurationSection configurationSection;

        public ReadModelConvention(EFCoreDataAccessConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()
                .Where(x => typeof(EntityReadModel).IsAssignableFrom(x.ClrType)))
            {
                entity.FindProperty(nameof(EntityReadModel.Id)).ValueGenerated = ValueGenerated.Never;
            }

            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var attr = entity.ClrType.GetCustomAttribute<ReadModelForEntityAttribute>();
                if (attr != null)
                {
                    var originalEntity = modelBuilder.Model.GetEntityTypes().FirstOrDefault(x => x.ClrType == attr.EntityType)
                                         ?? throw new InvalidOperationException($"Cannot map {entity.ClrType} as ReadModelForEntity for {attr.EntityType} because the latter is not part of the model.");

                    entity.FindProperty("Id").SetColumnName(originalEntity.FindProperty("Id").GetColumnBaseName());
                }
            }
        }
    }
}
