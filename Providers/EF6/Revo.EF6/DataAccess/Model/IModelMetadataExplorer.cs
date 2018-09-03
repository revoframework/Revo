using System;
using System.Collections.Generic;

namespace Revo.EF6.DataAccess.Model
{
    public interface IModelMetadataExplorer
    {
        IEnumerable<Type> EntityTypes { get; }
        IEnumerable<string> SchemaSpaces { get; }

        IEnumerable<Type> GetSchemaSpaceEntityTypes(string schemaSpace);
        string GetEntityTypeSchemaSpace(Type entityType);
        bool IsTypeMapped(Type entityType);
        string GetTableNameByClrType(Type entityType);
        Type GetClrTypeByTableName(string tableName);
        Type TryGetClrTypeByTableName(string tableName);
    }
}
