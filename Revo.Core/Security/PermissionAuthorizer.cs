using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Revo.Core.Security
{
    public class PermissionAuthorizer : IPermissionAuthorizer
    {
        private readonly IPermissionCache permissionCache;
        private readonly IRolePermissionResolver rolePermissionResolver;
        private readonly Dictionary<ClaimsIdentity, HashSet<Permission>> identityPermissions = new Dictionary<ClaimsIdentity, HashSet<Permission>>();

        public PermissionAuthorizer(IPermissionCache permissionCache,
            IRolePermissionResolver rolePermissionResolver)
        {
            this.permissionCache = permissionCache;
            this.rolePermissionResolver = rolePermissionResolver;
        }

        public bool CheckAuthorization(ClaimsIdentity identity, IReadOnlyCollection<Permission> requiredPermissions)
        {
            HashSet<Permission> permissions;
            if (!identityPermissions.TryGetValue(identity, out permissions))
            {
                permissions = new HashSet<Permission>();

                IEnumerable<Guid> roleIds = identity
                    .FindAll(identity.RoleClaimType)
                    .Select(x => Guid.Parse(x.Value));

                foreach (Guid roleId in roleIds)
                {
                    IEnumerable<Permission> rolePermissions = permissionCache.GetRolePermissions(roleId, rolePermissionResolver);
                    foreach (Permission permission in rolePermissions)
                    {
                        permissions.Add(permission);
                    }
                }

                identityPermissions.Add(identity, permissions);
            }

            return CheckAuthorization(permissions, requiredPermissions);
        }
        
        public bool CheckAuthorization(IReadOnlyCollection<Permission> availablePermissions,
            IReadOnlyCollection<Permission> requiredPermissions)
        {
            //TODO: this needs caching badly!
            PermissionTree permissionTree = new PermissionTree();
            permissionTree.Initialize(availablePermissions);

            foreach (Permission requiredPermission in requiredPermissions)
            {
                if (!permissionTree.AuthorizePermission(requiredPermission))
                {
                    return false;
                }
            }

            return true;
        }
    }
        
}
