using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
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
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var attr = entity.ClrType.GetCustomAttribute<ReadModelForEntityAttribute>();
                if (attr != null)
                {
                    var originalEntity = modelBuilder.Model.GetEntityTypes().FirstOrDefault(x => x.ClrType == attr.EntityType)
                                         ?? throw new InvalidOperationException($"Cannot map {entity.ClrType} as ReadModelForEntity for {attr.EntityType} because the latter is not part of the model.");

                    /*entity.Relational().TableName = originalEntity.Relational().TableName;
                    entity.FindProperty("Id").Relational().ColumnName = originalEntity.FindProperty("Id").Relational().ColumnName;

                    var entityBuilder = modelBuilder.Entity(entity.ClrType);
                    entityBuilder.HasOne(originalEntity.ClrType).WithOne().HasForeignKey(entity.ClrType, "Id");
                    entity.FindPrimaryKey().Relational().Name = originalEntity.FindPrimaryKey().Relational().Name;*/

                    entity.Relational().TableName = configurationSection.ReadModelTablePrefix + originalEntity.Relational().TableName + configurationSection.ReadModelTableSuffix;
                    entity.FindProperty("Id").Relational().ColumnName = originalEntity.FindProperty("Id").Relational().ColumnName;
                }
            }
        }
    }
}
