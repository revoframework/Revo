using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Security
{
    public class PermissionAuthorizationMatcher : IPermissionAuthorizationMatcher
    {
        public bool CheckAuthorization(IReadOnlyCollection<Permission> availablePermissions,
            IEnumerable<Permission> requiredPermissions)
        {
            foreach (Permission requiredPermission in requiredPermissions)
            {
                if (!AuthorizePermission(availablePermissions, requiredPermission))
                {
                    return false;
                }
            }

            return true;
        }

        private bool AuthorizePermission(IReadOnlyCollection<Permission> availablePermissions, Permission requiredPermission)
        {
            var matchingPermissions = availablePermissions
                .Where(x => x.PermissionTypeId == requiredPermission.PermissionTypeId);

            if (requiredPermission.ContextId != null)
            {
                matchingPermissions = matchingPermissions
                    .Where(x => x.ContextId == null
                                || x.ContextId == requiredPermission.ContextId);
            }

            if (requiredPermission.ResourceId != null)
            {
                matchingPermissions = matchingPermissions
                    .Where(x => x.ResourceId == null
                                || x.ResourceId == requiredPermission.ResourceId);
            }

            return matchingPermissions.Any();
        }
    }
        
}
