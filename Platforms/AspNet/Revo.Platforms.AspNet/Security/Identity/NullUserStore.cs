using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Revo.Core.Security;

namespace Revo.Platforms.AspNet.Security.Identity
{
    public class NullUserStore : IUserStore<IIdentityUser, Guid>
    {
        private const string NotImplementedMessage = "Cannot query users with default NullUserStore. If you want to use authentication or authorization features, you have to register your own non-fake implementation.";

        public IQueryable<IIdentityUser> Users => throw new NotImplementedException(NotImplementedMessage);

        public Task CreateAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task UpdateAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task DeleteAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IIdentityUser> FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IIdentityUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task SetPasswordHashAsync(IIdentityUser user, string passwordHash)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<string> GetPasswordHashAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<bool> HasPasswordAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task SetLockoutEndDateAsync(IIdentityUser user, DateTimeOffset lockoutEnd)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<int> IncrementAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task ResetAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<int> GetAccessFailedCountAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<bool> GetLockoutEnabledAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task SetLockoutEnabledAsync(IIdentityUser user, bool enabled)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task SetTwoFactorEnabledAsync(IIdentityUser user, bool enabled)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<bool> GetTwoFactorEnabledAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task AddToRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task RemoveFromRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IList<string>> GetRolesAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<bool> IsInRoleAsync(IIdentityUser user, string roleName)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IList<Claim>> GetClaimsAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task AddClaimAsync(IIdentityUser user, Claim claim)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task RemoveClaimAsync(IIdentityUser user, Claim claim)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public Task<IEnumerable<PermissionData>> GetUserPermissionsAsync(IIdentityUser user)
        {
            throw new NotImplementedException(NotImplementedMessage);
        }

        public void Dispose()
        {
        }
    }
}