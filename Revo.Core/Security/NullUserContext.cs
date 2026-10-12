using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security;

/// <summary>
/// User context with no authenticated user, used when no other implementation is registered
/// (e.g. in applications without ASP.NET Core integration).
/// </summary>
public class NullUserContext : IUserContext
{
    public bool IsAuthenticated => false;
    public Guid? UserId => null;

    public Task<IUser> GetUserAsync()
    {
        return Task.FromResult<IUser>(null);
    }

    public Task<IReadOnlyCollection<Permission>> GetPermissionsAsync()
    {
        return Task.FromResult((IReadOnlyCollection<Permission>) new List<Permission>());
    }
}
