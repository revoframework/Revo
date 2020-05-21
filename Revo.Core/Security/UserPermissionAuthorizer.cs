using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security
{
    public class UserPermissionAuthorizer : IUserPermissionAuthorizer
    {
        private readonly IUserContext userContext;
        private readonly IPermissionAuthorizationMatcher permissionAuthorizationMatcher;
        private readonly IUserPermissionResolver userPermissionResolver;

        public UserPermissionAuthorizer(IUserContext userContext,
            IPermissionAuthorizationMatcher permissionAuthorizationMatcher,
            IUserPermissionResolver userPermissionResolver)
        {
            this.userContext = userContext;
            this.permissionAuthorizationMatcher = permissionAuthorizationMatcher;
            this.userPermissionResolver = userPermissionResolver;
        }

        public async Task<bool> CheckAuthorizationAsync(IUser user, Guid permissionId, string resourceId = null, string contextId = null)
        {
            var userPermissions = await userPermissionResolver.GetUserPermissionsAsync(user);
            return permissionAuthorizationMatcher.CheckAuthorization(userPermissions,
                new[] { GetPermission(permissionId, resourceId, contextId) });
        }

        public async Task<bool> CheckAuthorizationAsync(IUser user, IEnumerable<Permission> permissions)
        {
            var userPermissions = await userPermissionResolver.GetUserPermissionsAsync(user);
            return permissionAuthorizationMatcher.CheckAuthorization(userPermissions,
                permissions);
        }

        public async Task<bool> CheckCurrentUserAuthorizationAsync(Guid permissionId, string resourceId = null, string contextId = null)
        {
            var userPermissions = await userContext.GetPermissionsAsync();
            return permissionAuthorizationMatcher.CheckAuthorization(userPermissions,
                new[] { GetPermission(permissionId, resourceId, contextId) });
        }

        public async Task<bool> CheckCurrentUserAuthorizationAsync(IEnumerable<Permission> permissions)
        {
            var userPermissions = await userContext.GetPermissionsAsync();
            return permissionAuthorizationMatcher.CheckAuthorization(userPermissions,
                permissions);
        }

        private Permission GetPermission(Guid permissionId, string resourceId, string contextId)
        {
            return new Permission(permissionId, resourceId, contextId);
        }
    }
}
