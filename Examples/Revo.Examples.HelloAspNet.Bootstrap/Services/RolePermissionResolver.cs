using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Revo.Core.Security;

namespace Revo.Examples.HelloAspNet.Bootstrap.Services
{
    public class RolePermissionResolver : IRolePermissionResolver
    {
        public IEnumerable<Permission> GetRolePermissions(Guid roleId)
        {
            return Enumerable.Empty<Permission>();
        }
    }
}