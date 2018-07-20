using System.Threading.Tasks;

namespace Revo.Core.Security
{
    public interface IUserPermissionAuthorizer
    {
        Task<bool> CheckAuthorizationAsync(IUser user, string permissionId, string resourceId = null, string contextId = null);
        Task<bool> CheckCurrentUserAuthorizationAsync(string permissionId, string resourceId = null, string contextId = null);
    }
}