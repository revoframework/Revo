using System;
using System.Collections.Generic;

namespace GTRevo.Platform.Security
{
    public interface IPermissionTypeRegistry
    {
        IEnumerable<PermissionType> PermissionTypes { get; }
        void RegisterPermissionType(PermissionType permissionType);
        PermissionType GetPermissionTypeById(Guid permissionId);
        PermissionType GetPermissionTypeByName(string permissionName);
    }
}
