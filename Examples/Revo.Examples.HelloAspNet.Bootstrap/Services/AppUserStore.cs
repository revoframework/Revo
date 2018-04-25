using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Revo.Core.Security;
using Revo.Platforms.AspNet.Security;
using Revo.Platforms.AspNet.Security.Identity;

namespace Revo.Examples.HelloAspNet.Bootstrap.Services
{
    public class AppUserStore : IAppUserStore
    {
        public void Dispose()
        {
        }

        public Task CreateAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<IIdentityUser> FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IIdentityUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(IIdentityUser user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasPasswordAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(IIdentityUser user, DateTimeOffset lockoutEnd)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetLockoutEnabledAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(IIdentityUser user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public Task SetTwoFactorEnabledAsync(IIdentityUser user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetTwoFactorEnabledAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task AddToRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetRolesAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsInRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public IQueryable<IIdentityUser> Users { get; }
        public Task<IList<Claim>> GetClaimsAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task AddClaimAsync(IIdentityUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(IIdentityUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<PermissionData>> GetUserPermissionsAsync(IIdentityUser user)
        {
            throw new NotImplementedException();
        }
    }
}