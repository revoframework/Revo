using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.ReadModel;

namespace GTRevo.Infrastructure.DataAcccess
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

            bool isView = entityName.EndsWith("View");
            if (isView)
            {
                entityName = entityName.Substring(0, entityName.Length - "View".Length);
            }

            bool isReadModel = entityName.EndsWith("ReadModel");
            if (isReadModel)
            {
                entityName = entityName.Substring(0, entityName.Length - "ReadModel".Length);
            }

            return entityName;
        }

        protected override string GetTableName(Type clrType, string entityName)
        {
            Type mappedType = GetOriginalMappeddType(clrType) ?? clrType;

            string namespacePrefix, columnPrefix;
            GetEntityPrefixes(mappedType, out namespacePrefix, out columnPrefix);

            string tableName = ConvertNameToSnakeCase(entityName);

            if (clrType.Name.EndsWith("View"))
            {
                tableName = "VW_" + tableName;
            }
            
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
