using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Revo.DataAccess.Entities;

namespace Revo.EFCore.DataAccess.Conventions
{
    public class PrefixConvention : IEFCoreConvention
    {
        public void Initialize(ModelBuilder modelBuilder)
        {
        }

        public void Finalize(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tablePrefixAttribute = entity.ClrType.GetCustomAttribute<TablePrefixAttribute>();

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
