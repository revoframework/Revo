using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Revo.Core.Security;
using Revo.Core.Security.ClaimBased;
using IUser = Revo.Core.Security.IUser;

namespace Revo.AspNetCore.Security
{
    public class AspNetCoreUserContext : IUserContext
    {
        private readonly IUserPermissionResolver userPermissionResolver;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IClaimsPrincipalUserResolver claimsPrincipalUserResolver;
        private readonly Lazy<Guid?> userIdLazy;
        private IReadOnlyCollection<Permission> userPermissions;
        private IUser user;

        public AspNetCoreUserContext(IUserPermissionResolver userPermissionResolver,
            IClaimsPrincipalUserResolver claimsPrincipalUserResolver,
            IHttpContextAccessor httpContextAccessor)
        {
            this.userPermissionResolver = userPermissionResolver;
            this.claimsPrincipalUserResolver = claimsPrincipalUserResolver;
            this.httpContextAccessor = httpContextAccessor;

            userIdLazy = new Lazy<Guid?>(() =>
            {
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
                {
                    var claimsPrincipal = httpContext.User;
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
                    var httpContext = httpContextAccessor.HttpContext;
                    var claimsPrincipal = httpContext.User;
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
