using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security.Identity
{
    public interface IAppUserStore : IUserStore<IUser, Guid>, IUserPasswordStore<IUser, Guid>,
        IUserLockoutStore<IUser, Guid>, IUserTwoFactorStore<IUser, Guid>,
        IUserRoleStore<IUser, Guid>, IQueryableUserStore<IUser, Guid>, IUserClaimStore<IUser, Guid>
    {
        Task<IEnumerable<PermissionData>> GetUserPermissionsAsync(IUser user);
    }
}
