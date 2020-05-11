using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Ninject;
using Revo.Core.Security;
using Revo.Core.Security.ClaimBased;
using IUser = Revo.Core.Security.IUser;

namespace Revo.AspNet.Security
{
    public class AspNetUserContext : IUserContext
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly IUserPermissionResolver userPermissionResolver;
        private readonly IClaimsPrincipalUserResolver claimsPrincipalUserResolver;
        private readonly Lazy<Guid?> userIdLazy;
        private IUser user;
        private IReadOnlyCollection<Permission> userPermissions;

        public AspNetUserContext(
            [Optional] IAuthenticationManager authenticationManager,
            IUserPermissionResolver userPermissionResolver,
            IClaimsPrincipalUserResolver claimsPrincipalUserResolver)
        {
            this.authenticationManager = authenticationManager;
            this.userPermissionResolver = userPermissionResolver;
            this.claimsPrincipalUserResolver = claimsPrincipalUserResolver;

            userIdLazy = new Lazy<Guid?>(() =>
            {
                if (authenticationManager.User?.Identity?.IsAuthenticated ?? false)
                {
                    var claimsPrincipal = authenticationManager.User;
                    return claimsPrincipalUserResolver.TryGetUserId(claimsPrincipal);
                }

                return null;
            });
        }

        public bool IsAuthenticated => UserId != null;
        public Guid? UserId => userIdLazy.Value;

        public async Task<IReadOnlyCollection<Permission>> GetPermissionsAsync()
        {
            if (userPermissions == null)
            {
                if (IsAuthenticated)
                {
                    IUser user = await GetUserAsync();
                    userPermissions = await userPermissionResolver.GetUserPermissionsAsync(user);
                }
                else
                {
                    userPermissions = new List<Permission>();
                }
            }

            return userPermissions;
        }

        public async Task<IUser> GetUserAsync()
        {
            if (user == null)
            {
                if (UserId != null)
                {
                    var claimsPrincipal = authenticationManager.User;
                    user = await claimsPrincipalUserResolver.GetUserAsync(claimsPrincipal);
                    if (user == null)
                    {
                        throw new InvalidOperationException($"GetUserAsync failed because the authenticated user with ID '{UserId}' could not be found");
                    }
                }
            }

            return user;
        }
    }
}
