using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
            foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(x => x.BaseType == null))
            {
                var tablePrefixAttribute = GetTablePrefixAttribute(entity);
                string tableName = tablePrefixAttribute?.NamespacePrefix?.Length > 0
                    ? $"{tablePrefixAttribute.NamespacePrefix}_{entity.GetTableName()}"
                    : entity.GetTableName();

                PrefixEntitiesRecursive(entity, tablePrefixAttribute, tableName);
            }
        }

        private void PrefixEntitiesRecursive(IMutableEntityType entity, TablePrefixAttribute entityPrefixAttribute, string tableName)
        {
            if (entity.BaseType == null)
            {
                entity.SetTableName(tableName);
            }

            PrefixColumnNames(entity, entityPrefixAttribute?.NamespacePrefix, entityPrefixAttribute?.ColumnPrefix);

            foreach (var child in entity.GetDerivedTypes())
            {
                if (child.BaseType != entity)
                {
                    continue;
                }

                var childPrefixAttribute = GetTablePrefixAttribute(child);
                PrefixEntitiesRecursive(child, childPrefixAttribute, tableName);
            }
        }

        private void PrefixColumnNames(IMutableEntityType entity, string namespacePrefix, string columnPrefix)
        {
            foreach (var property in entity.GetProperties())
            {
                if (property.DeclaringEntityType != entity)
                {
                    continue;
                }

                string propertyNamespacePrefix = namespacePrefix;
                string propertyColumnPrefix = columnPrefix;

                Type clrDeclaringType = property.PropertyInfo?.DeclaringType
                    ?? property.FieldInfo?.DeclaringType;
                TablePrefixAttribute clrTypePrefixAttribute;
                if (clrDeclaringType != null
                    && property.DeclaringEntityType.ClrType != clrDeclaringType
                    && (clrTypePrefixAttribute = GetTablePrefixAttribute(clrDeclaringType)) != null) //this might happen e.g. if clrDeclaringType is abstract but not 
                {
                    propertyNamespacePrefix = clrTypePrefixAttribute.NamespacePrefix;
                    propertyColumnPrefix = clrTypePrefixAttribute.ColumnPrefix;
                }

                if (propertyColumnPrefix?.Length > 0)
                {
                    property.SetColumnName($"{propertyColumnPrefix}_{property.GetColumnBaseName()}");
                }

                if (propertyNamespacePrefix?.Length > 0)
                {
                    property.SetColumnName($"{propertyNamespacePrefix}_{property.GetColumnBaseName()}");
                }
            }
        }

        private TablePrefixAttribute GetTablePrefixAttribute(IMutableEntityType entity)
        {
            var tablePrefixAttribute = entity.ClrType.GetCustomAttribute<TablePrefixAttribute>();
            var readModelForEntityAttribute = entity.ClrType.GetCustomAttribute<ReadModelForEntityAttribute>();

            if (tablePrefixAttribute == null && readModelForEntityAttribute != null)
            {
                tablePrefixAttribute = readModelForEntityAttribute.EntityType.GetCustomAttribute<TablePrefixAttribute>();
            }

            return tablePrefixAttribute;
        }

        private TablePrefixAttribute GetTablePrefixAttribute(Type entityType)
        {
            var tablePrefixAttribute = entityType.GetCustomAttribute<TablePrefixAttribute>();
            return tablePrefixAttribute;
        }
    }
}
