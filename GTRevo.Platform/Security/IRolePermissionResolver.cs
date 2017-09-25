using System;
using System.Collections.Generic;
using GTRevo.Core.Security;

namespace GTRevo.Platform.Security
{
    public interface IRolePermissionResolver
    {
        IEnumerable<Permission> GetRolePermissions(Guid roleId);
    }
}
