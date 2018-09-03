using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Revo.DataAccess.Entities;

namespace Revo.EF6.DataAccess.Model
{
    public class CustomStoreConvention : Convention,
        IStoreModelConvention<EntitySet>, IStoreModelConvention<EdmProperty>
    {
        public const string ClrTypeEntitySetAnnotationName = "ClrType";

        private readonly Dictionary<string, Type> configuredEntityTypes = new Dictionary<string, Type>();
        private readonly Dictionary<Type, ConventionTypeConfiguration> entityTypeConfigurations = new Dictionary<Type, ConventionTypeConfiguration>();

        public CustomStoreConvention()
        {
            Types().Configure(ConfigureType);
            Properties().Configure(ConfigureProperty);
        }

        public string CurrentSchemaSpace { get; set; } = "Default";

        public void Reset()
        {
            configuredEntityTypes.Clear();
            entityTypeConfigurations.Clear();
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
            item.AddAnnotation(ClrTypeEntitySetAnnotationName, GetEntityType(item.Name));
        }

        private void ConfigureProperty(ConventionPrimitivePropertyConfiguration config)
        {
        }
        
        private bool ConfigureTableType(Type clrType)
        {
            var mapping = GetDefaultMapping(clrType);
            Type existingType;
            if (configuredEntityTypes.TryGetValue(mapping.TableName.ToLowerInvariant(), out existingType)
                && existingType != clrType)
            {
                string newTypeSchema = EntityTypeDiscovery.DetectEntitySchemaSpace(clrType);
                string existingTypeSchema = EntityTypeDiscovery.DetectEntitySchemaSpace(existingType);

                if (newTypeSchema != CurrentSchemaSpace)
                {
                    return false;
                }
                else if (existingTypeSchema != CurrentSchemaSpace)
                {
                    var existingTypeConfiguration = entityTypeConfigurations[existingType];

                    //existingTypeConfiguration.Ignore();
                    //fugly hack, something's probably changed in EF

                    object entityTypeConfigurationFieldVal = existingTypeConfiguration.GetType()
                        .GetField("_entityTypeConfiguration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(existingTypeConfiguration);
                    object complexTypeConfigurationFieldVal = existingTypeConfiguration.GetType()
                        .GetField("_complexTypeConfiguration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(existingTypeConfiguration);

                    //if (entityTypeConfigurationFieldVal == null && complexTypeConfigurationFieldVal == null)
                    //{
                        /*ModelConfiguration*/ object modelConfigurationFieldVal = existingTypeConfiguration.GetType()
                            .GetField("_modelConfiguration", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(existingTypeConfiguration);
                        modelConfigurationFieldVal.GetType().GetMethod("Ignore")
                            .Invoke(modelConfigurationFieldVal, new[] {existingType});
                    //}

                    entityTypeConfigurations.Remove(existingType);
                    configuredEntityTypes.Remove(mapping.TableName.ToLowerInvariant());
                }
                else
                {
                    throw new InvalidOperationException("Conflicting entity types");
                }
            }

            if (!configuredEntityTypes.ContainsKey(clrType.Name.ToLowerInvariant()))
            {
                configuredEntityTypes.Add(clrType.Name.ToLowerInvariant(), clrType);
            }

            if (!configuredEntityTypes.ContainsKey(mapping.TableName.ToLowerInvariant()))
            {
                configuredEntityTypes.Add(mapping.TableName.ToLowerInvariant(), clrType);
            }

            return true;
        }

        protected virtual TableMapping GetDefaultMapping(Type clrType)
        {
            string entityName = GetEntityName(clrType);
            string tableName = GetTableName(clrType, entityName);
            
            return new TableMapping()
            {
                TableName = tableName,
                EntityName = entityName
            };
        }

        protected virtual string GetTableName(Type clrType, string entityName)
        {
            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(clrType, out namespacePrefix, out columnPrefix);

            string tableName = ConvertNameToSnakeCase(entityName);

            if (namespacePrefix != null)
            {
                tableName = namespacePrefix + "_" + tableName;
            }

            return tableName;
        }

        private void ConfigureType(ConventionTypeConfiguration config)
        {
            if (!ConfigureTableType(config.ClrType))
            {
                config.Ignore();
                return;
            }
            
            entityTypeConfigurations[config.ClrType] = config;
            var mapping = GetDefaultMapping(config.ClrType);
            config.ToTable(mapping.TableName);
        }

        private Type GetEntityType(string typeName)
        {
            /*var entityTypes = AppDomain.CurrentDomain.GetAssemblies()
                   .SelectMany(t => t.GetTypes())
                   .Where(t => t.IsClass && t.Name.Equals(typeName));*/

            var entityTypes = configuredEntityTypes.Where(
                   t => t.Value.IsClass && t.Key.Equals(typeName.ToLowerInvariant()))
                   .ToList();

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

        protected virtual void GetEntityPrefixes(Type entityType, out string namespacePrefix, out string columnPrefix)
        {
            TablePrefixAttribute tablePrefixAttribute = (TablePrefixAttribute)
                entityType.GetCustomAttributes(typeof(TablePrefixAttribute)).FirstOrDefault();

            columnPrefix = tablePrefixAttribute?.ColumnPrefix;
            namespacePrefix = tablePrefixAttribute?.NamespacePrefix;
        }

        private string GetColumnName(Type entityType, string propertyName)
        {
            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(entityType, out namespacePrefix, out columnPrefix);

            string columnName = propertyName;
            string entityName = GetEntityName(entityType);

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

        protected virtual string GetEntityName(Type clrType)
        {
            return clrType.Name;
        }

        protected string ConvertNameToSnakeCase(string pascalCasedName)
        {
            return Regex.Replace(pascalCasedName, "([A-Z])([A-Z][a-z])|([a-z0-9])(?=[A-Z])", "$1$3_$2")
                .ToUpper();
        }

        protected struct TableMapping
        {
            public string TableName { get; set; }
            public string EntityName { get; set; }
        }

        /*private ClrType GetRealDeclaringType(PropertyInfo propertyInfo)
        {
            Stack<ClrType> typeHierarchy = new Stack<ClrType>();
            typeHierarchy.Push(propertyInfo.ReflectedType);

            while (typeHierarchy.First() != propertyInfo.DeclaringType)
            {
                typeHierarchy.Push(typeHierarchy.First().BaseType);
            }

            ClrType type;
            do
            {
                type = typeHierarchy.First();
                typeHierarchy.Pop();
            }
            while (!configuredEntityTypes.Values.Contains(type));

            return type;
        }*/
    }
}
