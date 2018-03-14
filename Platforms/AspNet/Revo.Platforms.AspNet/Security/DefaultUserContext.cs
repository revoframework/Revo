using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject;
using Revo.Core.Security;
using Revo.Platforms.AspNet.Security.Identity;
using IUser = Revo.Core.Security.IUser;

namespace Revo.Platforms.AspNet.Security
{
    public class DefaultUserContext : IUserContext
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly AppUserManager userManager;
        private readonly Lazy<Guid?> userIdLazy;
        private IIdentityUser user;
        private IEnumerable<Permission> userPermissions;

        public DefaultUserContext([Optional] IAuthenticationManager authenticationManager,
            AppUserManager userManager)
        {
            this.authenticationManager = authenticationManager;
            this.userManager = userManager;

            userIdLazy = new Lazy<Guid?>(() =>
            {
                string userIdString = authenticationManager?.User?.Identity?.GetUserId<string>();
                return userIdString != null ? ((Guid?) Guid.Parse(userIdString)) : null;
            });
        }

        public bool IsAuthenticated
        {
            get
            {
                return authenticationManager?.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public Guid? UserId => userIdLazy.Value;

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            if (userPermissions == null)
            {
                if (IsAuthenticated)
                {
                    IUser user = await GetUserAsync();
                    userPermissions = await userManager.GetUserPermissionsAsync((IIdentityUser) user);
                }
                else
                {
                    userPermissions = Enumerable.Empty<Permission>();
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
                    user = await userManager.FindByIdAsync(UserId.Value);
                    if (user == null)
                    {
                        throw new InvalidOperationException($"GetUserAsync failed because the authenticated user with ID '{UserId.Value}' could not be found");
                    }
                }
            }

            return (IUser) user;
        }
    }
}
