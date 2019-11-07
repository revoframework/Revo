using System;
using System.Linq;
using Revo.Domain.ReadModel;

namespace Revo.EF6.DataAccess.Model
{
    public class CustomStoreConventionEx : CustomStoreConvention
    { 
        protected override string GetEntityName(Type clrType)
        {
            Type mappedType = GetOriginalMappeddType(clrType) ?? clrType;

            string entityName = mappedType.Name;
            int chevronOpenI = entityName.IndexOf('<');
            if (chevronOpenI != -1)
            {
                entityName = entityName.Substring(0, chevronOpenI);
            }
            
            return entityName;
        }

        protected override string GetTableName(Type clrType, string entityName)
        {
            Type mappedType = GetOriginalMappeddType(clrType) ?? clrType;

            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(mappedType, out namespacePrefix, out columnPrefix);

            string tableName = ConvertNameToSnakeCase(entityName);
            
            if (namespacePrefix != null)
            {
                tableName = namespacePrefix + "_" + tableName;
            }
            
            return tableName;
        }

        protected Type GetOriginalMappeddType(Type clrType)
        {
            var attr = (ReadModelForEntityAttribute)clrType.GetCustomAttributes(typeof(ReadModelForEntityAttribute), false).FirstOrDefault();
            return attr?.EntityType;
        }

        protected override void GetEntityPrefixes(Type entityType, out string namespacePrefix, out string columnPrefix)
        {
            Type mappedType = GetOriginalMappeddType(entityType) ?? entityType;
            base.GetEntityPrefixes(mappedType, out namespacePrefix, out columnPrefix);
        }
    }
}
