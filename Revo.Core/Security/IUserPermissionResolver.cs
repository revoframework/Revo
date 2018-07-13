using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security
{
    public interface IUserPermissionResolver
    {
        Task<IReadOnlyCollection<Permission>> GetUserPermissionsAsync(IUser user);
    }
}