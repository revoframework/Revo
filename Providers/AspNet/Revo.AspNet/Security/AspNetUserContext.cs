using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using Revo.AspNet.Security.Identity;
using Revo.Core.Security;
using IUser = Revo.Core.Security.IUser;

namespace Revo.AspNet.Security
{
    public class AspNetUserContext : IUserContext
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly IUserPermissionResolver userPermissionResolver;
        private readonly IUserStore<IIdentityUser, Guid> userStore;
        private readonly Lazy<Guid?> userIdLazy;
        private IIdentityUser user;
        private IReadOnlyCollection<Permission> userPermissions;

        public AspNetUserContext(
            [Optional] IAuthenticationManager authenticationManager,
            IUserPermissionResolver userPermissionResolver,
            IUserStore<IIdentityUser, Guid> userStore)
        {
            this.authenticationManager = authenticationManager;
            this.userPermissionResolver = userPermissionResolver;
            this.userStore = userStore;

            userIdLazy = new Lazy<Guid?>(() =>
            {
                string userIdString = authenticationManager?.User?.Identity?.GetUserId<string>();
                return userIdString != null ? ((Guid?) Guid.Parse(userIdString)) : null;
            });
        }

        public bool IsAuthenticated => authenticationManager?.User?.Identity?.IsAuthenticated ?? false;

        public Guid? UserId => userIdLazy.Value;

        public async Task<IReadOnlyCollection<Permission>> GetPermissionsAsync()
        {
            if (userPermissions == null)
            {
                if (IsAuthenticated)
                {
                    IIdentityUser user = await GetUserInternalAsync();
                    userPermissions = await userPermissionResolver.GetUserPermissionsAsync(user.User);
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
            return (await GetUserInternalAsync()).User;
        }

        private async Task<IIdentityUser> GetUserInternalAsync()
        {
            if (user == null)
            {
                if (UserId != null)
                {
                    user = await userStore.FindByIdAsync(UserId.Value);
                    if (user == null)
                    {
                        throw new InvalidOperationException($"GetUserAsync failed because the authenticated user with ID '{UserId.Value}' could not be found");
                    }
                }
            }

            return user;
        }
    }
}
