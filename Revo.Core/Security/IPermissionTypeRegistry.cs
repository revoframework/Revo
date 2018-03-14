using System;
using System.Collections.Generic;

namespace Revo.Core.Security
{
    public interface IPermissionTypeRegistry
    {
        IEnumerable<PermissionType> PermissionTypes { get; }
        void RegisterPermissionType(PermissionType permissionType);
        PermissionType GetPermissionTypeById(Guid permissionId);
        PermissionType GetPermissionTypeByName(string permissionName);
    }
}
