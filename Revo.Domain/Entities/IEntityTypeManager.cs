using System;
using System.Collections.Generic;

namespace Revo.Domain.Entities
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
