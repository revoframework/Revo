using System;
using System.Collections.Generic;
using Revo.Core.Security;

namespace Revo.Platforms.AspNet.Security
{
    public interface IPermissionTypeRegistry
    {
        IEnumerable<PermissionType> PermissionTypes { get; }
        void RegisterPermissionType(PermissionType permissionType);
        PermissionType GetPermissionTypeById(Guid permissionId);
        PermissionType GetPermissionTypeByName(string permissionName);
    }
}
