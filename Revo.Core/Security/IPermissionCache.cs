using System;
using System.Collections.Generic;

namespace Revo.Core.Security
{
    public interface IPermissionCache
    {
        IEnumerable<Permission> GetRolePermissions(Guid roleId, IRolePermissionResolver rolePermissionResolver);
        void Invalidate();
    }
}