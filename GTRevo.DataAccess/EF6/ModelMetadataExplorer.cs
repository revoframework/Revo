using System;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace GTRevo.DataAccess.EF6
{
    public class ModelMetadataExplorer : IModelMetadataExplorer
    {
        private readonly Func<EntityContext> dbContextFunc;

        public ModelMetadataExplorer(Func<EntityContext> dbContextFunc)
        {
            this.dbContextFunc = dbContextFunc;
        }

        public string GetTableNameByClrType(Type entityType)
        {
            EntityContainer container = GetEntityContainer();
            EntitySet entitySet = container.EntitySets.FirstOrDefault(
                x =>
                {
                    var prop = x.MetadataProperties.First(
                        y => y.Name == CustomStoreConvention.CLR_TYPE_ENTITY_SET_ANNOTATION_NAME);
                    Type clrType = (Type)prop.Value;
                    return clrType == entityType;
                });

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
                y => y.Name == CustomStoreConvention.CLR_TYPE_ENTITY_SET_ANNOTATION_NAME);
            Type clrType = (Type)prop.Value;

            return clrType;
        }

        private EntityContainer GetEntityContainer()
        {
            EntityContext dbContext = dbContextFunc(); //TODO: dispose or not?

            ObjectContext objectContext = ((IObjectContextAdapter) dbContext).ObjectContext;
            EntityContainerMapping containerMapping = objectContext.MetadataWorkspace
                .GetItem<EntityContainerMapping>(objectContext.DefaultContainerName, DataSpace.CSSpace);
            EntityContainer container = containerMapping.StoreEntityContainer;
            return container;
        }
    }
}
