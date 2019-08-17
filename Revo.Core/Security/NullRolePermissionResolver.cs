using System;
using System.Collections.Generic;

namespace Revo.Core.Security
{
    public class NullRolePermissionResolver : IRolePermissionResolver
    {
        public IReadOnlyCollection<Permission> GetRolePermissions(Guid roleId)
        {
            return new List<Permission>();
        }
    }
}
