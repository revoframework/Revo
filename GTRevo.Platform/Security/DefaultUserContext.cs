using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Platform.Security.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace GTRevo.Platform.Security
{
    public class DefaultUserContext : IUserContext
    {
        private readonly IAuthenticationManager authenticationManager;
        private readonly AppUserManager userManager;
        private IUser user;
        private IEnumerable<Permission> userPermissions;
        private Lazy<Guid?> userIdLazy;

        public DefaultUserContext(IAuthenticationManager authenticationManager,
            AppUserManager userManager)
        {
            this.authenticationManager = authenticationManager;
            this.userManager = userManager;

            userIdLazy = new Lazy<Guid?>(() =>
            {
                string userIdString = authenticationManager.User?.Identity?.GetUserId<string>();
                return userIdString != null ? ((Guid?) Guid.Parse(userIdString)) : null;
            });
        }

        public bool IsAuthenticated
        {
            get
            {
                return authenticationManager.User?.Identity?.IsAuthenticated ?? false;
            }
        }

        public Guid? UserId
        {
            get { return userIdLazy.Value; }
        }

        public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            if (userPermissions == null && IsAuthenticated)
            {
                IUser user = await GetUserAsync();
                userPermissions = await userManager.GetUserPermissionsAsync(user);
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

            return user;
        }
    }
}
