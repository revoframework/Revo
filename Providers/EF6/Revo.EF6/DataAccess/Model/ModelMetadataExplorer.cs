using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Revo.Core.Collections;
using Revo.Core.Lifecycle;
using Revo.EF6.DataAccess.Entities;

namespace Revo.EF6.DataAccess.Model
{
    public class ModelMetadataExplorer : IModelMetadataExplorer, IApplicationStartedListener
    {
        private readonly IDbContextFactory dbContextFactory;
        private readonly EntityTypeDiscovery entityTypeDiscovery;
        private Dictionary<Type, string> entityTypeSchemaSpaces;
        private MultiValueDictionary<string, Type> schemaSpaceEntityTypes;

        public ModelMetadataExplorer(EntityTypeDiscovery entityTypeDiscovery,
            IDbContextFactory dbContextFactory)
        {
            this.entityTypeDiscovery = entityTypeDiscovery;
            this.dbContextFactory = dbContextFactory;
        }

        public IEnumerable<Type> EntityTypes => entityTypeSchemaSpaces.Keys;
        public IEnumerable<string> SchemaSpaces { get; private set; }

        public void OnApplicationStarted()
        {
            EnsureLoaded();
        }

        public string GetEntityTypeSchemaSpace(Type entityType)
        {
            EnsureLoaded();

            string schemaSpace;
            if (entityTypeSchemaSpaces.TryGetValue(entityType, out schemaSpace))
            {
                return schemaSpace;
            }

            throw new ArgumentException($"Entity type not found: {entityType.FullName}");
        }

        public IEnumerable<Type> GetSchemaSpaceEntityTypes(string schemaSpace)
        {
            EnsureLoaded();

            IReadOnlyCollection<Type> entityTypes;
            if (schemaSpaceEntityTypes.TryGetValue(schemaSpace, out entityTypes))
            {
                return entityTypes;
            }

            return new List<Type>();
        }

        public bool IsTypeMapped(Type entityType)
        {
            EntitySet entitySet = FindEntitySetForType(entityType);
            return entitySet != null;
        }

        public string GetTableNameByClrType(Type entityType)
        {
            EntitySet entitySet = FindEntitySetForType(entityType);

            if (entitySet == null)
            {
                throw new ArgumentException($"Table mapping for CLR type '{entityType.FullName}' was not found");
            }

            return entitySet.Table;
        }

        public Type GetClrTypeByTableName(string tableName)
        {
            Type clrType = TryGetClrTypeByTableName(tableName);

            if (clrType == null)
            {
                throw new ArgumentException($"CLR type mapping for table named '{tableName}' was not found");
            }

            return clrType;
        }

        public Type TryGetClrTypeByTableName(string tableName)
        {
            EntityContainer container = GetEntityContainer();
            EntitySet entitySet = container.EntitySets.FirstOrDefault(x => x.Table == tableName);

            if (entitySet == null)
            {
                return null;
            }

            var prop = entitySet.MetadataProperties.First(
                y => y.Name == CustomStoreConvention.ClrTypeEntitySetAnnotationName);
            Type clrType = (Type)prop.Value;

            return clrType;
        }

        private void EnsureLoaded()
        {
            if (entityTypeSchemaSpaces == null)
            {
                DiscoverEntityTypes();
            }
        }

        private void DiscoverEntityTypes()
        {
            schemaSpaceEntityTypes = entityTypeDiscovery.DiscoverEntities();
            entityTypeSchemaSpaces = schemaSpaceEntityTypes
                .SelectMany(x => x.Value, (key, entityType) => new {SchemaSpace = key.Key, EntityType = entityType})
                .ToDictionary(x => x.EntityType, x => x.SchemaSpace);
            SchemaSpaces = schemaSpaceEntityTypes.Keys.ToList();
        }

        private EntityContainer GetEntityContainer()
        {
            DbContext dbContext = dbContextFactory.CreateContext("Default" /* TODO */); //TODO: dispose or not?

            ObjectContext objectContext = ((IObjectContextAdapter) dbContext).ObjectContext;
            EntityContainerMapping containerMapping = objectContext.MetadataWorkspace
                .GetItem<EntityContainerMapping>(objectContext.DefaultContainerName, DataSpace.CSSpace);
            EntityContainer container = containerMapping.StoreEntityContainer;
            return container;
        }

        private EntitySet FindEntitySetForType(Type entityType)
        {
            EntityContainer container = GetEntityContainer();
            return container.EntitySets.FirstOrDefault(
                x =>
                {
                    var prop = x.MetadataProperties.First(
                        y => y.Name == CustomStoreConvention.ClrTypeEntitySetAnnotationName);
                    Type clrType = (Type)prop.Value;
                    return clrType == entityType;
                });
        }
    }
}
