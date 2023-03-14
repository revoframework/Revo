using System;
using System.Collections.Generic;

namespace Revo.Domain.Entities
{
    public interface IEntityTypeManager
    {
        IEnumerable<DomainClassInfo> DomainEntities { get; }

        void ClearCache();
        DomainClassInfo GetClassInfoByClrType(Type clrType);
        DomainClassInfo GetClassInfoByClassId(Guid classId);
        DomainClassInfo TryGetClassInfoByClrType(Type clrType);
        DomainClassInfo TryGetClassInfoByClassId(Guid classId);
    }
}
