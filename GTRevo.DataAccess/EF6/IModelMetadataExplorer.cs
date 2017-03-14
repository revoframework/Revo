using System;

namespace GTRevo.DataAccess.EF6
{
    public interface IModelMetadataExplorer
    {
        string GetTableNameByClrType(Type entityType);
        Type GetClrTypeByTableName(string tableName);
    }
}
