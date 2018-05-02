using System;
using System.Collections.Generic;
using Revo.Domain.Entities.Attributes;

namespace Revo.Domain.Entities
{
    public interface IEntityTypeManager
    {
        IEnumerable<Type> DomainEntities { get; }

        DomainClassIdAttribute GetClassIdByClrType(Type clrType);
        Type GetClrTypeByClassId(Guid classId);
        DomainClassIdAttribute TryGetClassIdByClrType(Type clrType);
        Type TryGetClrTypeByClassId(Guid classId);
    }
}
