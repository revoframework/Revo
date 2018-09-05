using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class PrefixConvention : EFCoreConventionBase
    {
        public override void Initialize(ModelBuilder modelBuilder)
        {
        }

        public override void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tablePrefixAttribute = entity.ClrType.GetCustomAttribute<TablePrefixAttribute>();
                var readModelForEntityAttribute = entity.ClrType.GetCustomAttribute<ReadModelForEntityAttribute>();

                if (tablePrefixAttribute == null && readModelForEntityAttribute != null)
                {
                    tablePrefixAttribute = readModelForEntityAttribute.EntityType.GetCustomAttribute<TablePrefixAttribute>();
                }

                if (tablePrefixAttribute?.NamespacePrefix?.Length > 0)
                {
                    entity.Relational().TableName = $"{tablePrefixAttribute.NamespacePrefix}_{entity.Relational().TableName}";
                }

                foreach (var property in entity.GetProperties())
                {
                    if (tablePrefixAttribute?.ColumnPrefix?.Length > 0)
                    {
                        property.Relational().ColumnName = $"{tablePrefixAttribute.ColumnPrefix}_{property.Relational().ColumnName}";
                    }

                    if (tablePrefixAttribute?.NamespacePrefix?.Length > 0)
                    {
                        property.Relational().ColumnName = $"{tablePrefixAttribute.NamespacePrefix}_{property.Relational().ColumnName}";
                    }
                }
            }
        }
    }
}
