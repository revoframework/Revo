using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security
{
    public interface IRolePermissionResolver
    {
        IReadOnlyCollection<Permission> GetRolePermissions(Guid roleId);
    }
}
