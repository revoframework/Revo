using System;
using System.Collections.Generic;
using Revo.Core.Security;

namespace Revo.Platforms.AspNet.Security
{
    public interface IRolePermissionResolver
    {
        IEnumerable<Permission> GetRolePermissions(Guid roleId);
    }
}
