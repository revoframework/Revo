using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace GTRevo.DataAccess.EF6
{
    public class CustomStoreConvention : Convention,
        IStoreModelConvention<EntitySet>, IStoreModelConvention<EdmProperty>
    {
        public const string CLR_TYPE_ENTITY_SET_ANNOTATION_NAME = "ClrType";

        private Dictionary<string, Type> configuredEntityTypes = new Dictionary<string, Type>();

        public CustomStoreConvention()
        {
            Types().Configure(ConfigureType);
            Properties().Configure(ConfigureProperty);
        }

        private Type GetRealDeclaringType(PropertyInfo propertyInfo)
        {
            Stack<Type> typeHierarchy = new Stack<Type>();
            typeHierarchy.Push(propertyInfo.ReflectedType);

            while (typeHierarchy.First() != propertyInfo.DeclaringType)
            {
                typeHierarchy.Push(typeHierarchy.First().BaseType);
            }

            Type type;
            do
            {
                type = typeHierarchy.First();
                typeHierarchy.Pop();
            }
            while (!configuredEntityTypes.Values.Contains(type));

            return type;
        }

        private void ConfigureProperty(ConventionPrimitivePropertyConfiguration config)
        {
            /*string name = config.ClrPropertyInfo.Name;
            Type realDeclaringType = GetRealDeclaringType(config.ClrPropertyInfo);

            if (name == "ID")
            {
                name = realDeclaringType.Name + "Id";
            }
            
            name = GetColumnName(realDeclaringType, name);

            if (name.EndsWith("_ID"))
            {
                name = name.Substring(0, name.Length - 3) + "Id";
            }

            //config.HasColumnName(name);*/
        }

        private void ConfigureType(ConventionTypeConfiguration config)
        {
            if (!configuredEntityTypes.Values.Contains(config.ClrType))
            {
                configuredEntityTypes.Add(config.ClrType.Name, config.ClrType);
            }

            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(config.ClrType, out namespacePrefix, out columnPrefix);

            string tableName;
            bool isView;
            ConvertTypeNameToEntityName(config.ClrType.Name, out tableName, out isView);

            tableName = ConvertNameToSnakeCase(tableName);

            if (namespacePrefix != null)
            {
                tableName = namespacePrefix + "_" + (isView ? "VW_" : "") + tableName;
            }

            config.ToTable(tableName);

            if (!configuredEntityTypes.ContainsKey(tableName))
            {
                configuredEntityTypes.Add(tableName, config.ClrType);
            }
        }

        public void Apply(EdmProperty item, DbModel model)
        {
            /*var config = item.MetadataProperties.FirstOrDefault(x => x.Name == "Configuration")?.Value;
            if (config != null)
            {
                System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive.PrimitivePropertyConfiguration
                //config.HasColumnName()
            }*/

            string preferredName = (string)item.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value;

            if (item.Name != preferredName)
            {
                return; //workaround, seems to behave similarly to accessing item configuration in metadata (which is not possible since the used class is internal)
            }

            Type entityType = GetEntityType(item.DeclaringType.Name);
            item.Name = GetColumnName(entityType, item.Name);

            InheritedColumnNameAnnotation inheritedColumnNameAnnotation =
                (InheritedColumnNameAnnotation)item.MetadataProperties.FirstOrDefault(x => x.Name == "http://schemas.microsoft.com/ado/2013/11/edm/customannotation:InheritedColumnName")?.Value;
            if (inheritedColumnNameAnnotation != null && entityType == inheritedColumnNameAnnotation.BaseType)
            {
                item.Name = inheritedColumnNameAnnotation.Name;
            }

            if (item.Name.ToLower().EndsWith("_id"))
            {
                item.Name = item.Name.Substring(0, item.Name.Length - 3) + "Id";
            }
        }

        public void Apply(EntitySet item, DbModel model)
        {
            item.AddAnnotation(CLR_TYPE_ENTITY_SET_ANNOTATION_NAME, GetEntityType(item.Name));

            /*string namespacePrefix, columnPrefix;
            GetEntityPrefixes(GetEntityType(item.Name), out namespacePrefix, out columnPrefix);

            string tableName = item.Name;
            tableName = ConvertNameToSnakeCase(tableName);

            if (namespacePrefix != null)
            {
                tableName = namespacePrefix + "_" + tableName;
            }
            
            //item.Table = tableName;*/
        }

        private Type GetEntityType(string typeName)
        {
            /*var entityTypes = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(t => t.GetTypes())
                   .Where(t => t.IsClass && t.Name.Equals(typeName));*/

            var entityTypes = configuredEntityTypes.Where(
                   t => t.Value.IsClass && t.Key.Equals(typeName));

            if (entityTypes.Count() == 0)
            {
                throw new InvalidOperationException("No corresponding CLR type found for entity type named: " + typeName);
            }
            else if (entityTypes.Count() > 1)
            {
                throw new InvalidOperationException("More than one corresponding CLR type found for entity type named: " + typeName);
            }

            return entityTypes.First().Value;
        }

        private void GetEntityPrefixes(Type entityType, out string namespacePrefix, out string columnPrefix)
        {
            CustomAttributeData tablePrefixAttribute = entityType.CustomAttributes.FirstOrDefault(
                x => x.AttributeType == typeof(TablePrefixAttribute));

            columnPrefix = (string)tablePrefixAttribute?.NamedArguments.FirstOrDefault(
                    x => x.MemberName == nameof(TablePrefixAttribute.ColumnPrefix)).TypedValue.Value;
            namespacePrefix = (string)tablePrefixAttribute?.NamedArguments.FirstOrDefault(
                x => x.MemberName == nameof(TablePrefixAttribute.NamespacePrefix)).TypedValue.Value;
        }

        private string GetColumnName(Type entityType, string propertyName)
        {
            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(entityType, out namespacePrefix, out columnPrefix);

            string columnName = propertyName;

            string entityName;
            bool isView;
            ConvertTypeNameToEntityName(entityType.Name, out entityName, out isView);

            if (columnName.ToLower() == "id")
            {
                columnName = entityName + "Id";
            }

            if (columnPrefix != null)
            {
                columnName = columnPrefix + "_" + columnName;
            }

            if (namespacePrefix != null)
            {
                columnName = namespacePrefix + "_" + columnName;
            }

            return columnName;
        }

        private void ConvertTypeNameToEntityName(string typeName, out string entityName, out bool isView)
        {
            entityName = typeName;

            isView = typeName.EndsWith("View");
            if (isView)
            {
                entityName = entityName.Substring(0, entityName.Length - "View".Length);
            }
            
            bool isReadModel = typeName.EndsWith("ReadModel");
            if (isReadModel)
            {
                entityName = entityName.Substring(0, entityName.Length - "ReadModel".Length);
            }
        }

        private string ConvertNameToSnakeCase(string pascalCasedName)
        {
            return Regex.Replace(pascalCasedName, "([A-Z])([A-Z][a-z])|([a-z0-9])(?=[A-Z])", "$1$3_$2")
                .ToUpper();
        }
    }
}
