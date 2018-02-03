using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Revo.Platforms.AspNet.Security.Identity
{
    public interface IAppUserStore : IUserStore<IIdentityUser, Guid>, IUserPasswordStore<IIdentityUser, Guid>,
        IUserLockoutStore<IIdentityUser, Guid>, IUserTwoFactorStore<IIdentityUser, Guid>,
        IUserRoleStore<IIdentityUser, Guid>, IQueryableUserStore<IIdentityUser, Guid>, IUserClaimStore<IIdentityUser, Guid>
    {
        Task<IEnumerable<PermissionData>> GetUserPermissionsAsync(IIdentityUser user);
    }
}
