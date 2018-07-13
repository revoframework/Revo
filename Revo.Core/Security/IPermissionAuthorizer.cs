using System.Collections.Generic;
using System.Security.Claims;

namespace Revo.Core.Security
{
    public interface IPermissionAuthorizer
    {
        bool CheckAuthorization(ClaimsIdentity identity, IReadOnlyCollection<Permission> requiredPermissions);
        bool CheckAuthorization(IReadOnlyCollection<Permission> availablePermissions, IReadOnlyCollection<Permission> requiredPermissions);
    }
}