using System;
using System.Collections.Generic;

namespace GTRevo.Platform.Security
{
    public interface IRolePermissionResolver
    {
        IEnumerable<Permission> GetRolePermissions(Guid roleId);
    }
}
