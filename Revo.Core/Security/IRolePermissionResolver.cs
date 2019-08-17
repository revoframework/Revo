using System;
using System.Collections.Generic;

namespace Revo.Core.Security
{
    public interface IRolePermissionResolver
    {
        IReadOnlyCollection<Permission> GetRolePermissions(Guid roleId);
    }
}
