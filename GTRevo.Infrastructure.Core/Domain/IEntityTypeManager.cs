using System;
using System.Collections.Generic;

namespace GTRevo.Infrastructure.Domain
{
    public interface IEntityTypeManager
    {
        IEnumerable<Type> DomainEntities { get; }

        Guid GetClassIdByClrType(Type clrType);
        Type GetClrTypeByClassId(Guid classId);
    }
}
