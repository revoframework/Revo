using System;
using System.Collections.Generic;
using GTRevo.Core.Security;

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
