using System.Collections.Generic;

namespace Revo.Core.Security
{
    public interface IPermissionAuthorizationMatcher
    {
        bool CheckAuthorization(IReadOnlyCollection<Permission> availablePermissions,
            IEnumerable<Permission> requiredPermissions);
    }
}