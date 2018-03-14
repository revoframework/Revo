using System;
using System.Collections.Generic;

namespace Revo.Core.Security
{
    public interface IRolePermissionResolver
    {
        IEnumerable<Permission> GetRolePermissions(Guid roleId);
    }
}
