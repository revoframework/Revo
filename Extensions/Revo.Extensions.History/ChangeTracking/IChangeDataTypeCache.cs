using System;

namespace Revo.Extensions.History.ChangeTracking
{
    public interface IChangeDataTypeCache
    {
        string GetChangeDataTypeName(Type clrType);
        Type GetClrChangeDataType(string changeDataTypeName);
    }
}