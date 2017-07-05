using System;
using System.Collections.Generic;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IEntityTypeManager
    {
        IEnumerable<Type> DomainEntities { get; }

        Guid GetClassIdByClrType(Type clrType);
        Type GetClrTypeByClassId(Guid classId);
        Guid? TryGetClassIdByClrType(Type clrType);
        Type TryGetClrTypeByClassId(Guid classId);
    }
}
