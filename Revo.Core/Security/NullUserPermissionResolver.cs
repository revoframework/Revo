using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security
{
    public class NullUserPermissionResolver : IUserPermissionResolver
    {
        public Task<IReadOnlyCollection<Permission>> GetUserPermissionsAsync(IUser user)
        {
            return Task.FromResult((IReadOnlyCollection<Permission>) new List<Permission>());
        }
    }
}
