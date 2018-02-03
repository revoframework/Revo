using System;

namespace Revo.Infrastructure.History.ChangeTracking
{
    public interface IChangeDataTypeCache
    {
        string GetChangeDataTypeName(Type clrType);
        Type GetClrChangeDataType(string changeDataTypeName);
    }
}