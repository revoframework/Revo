using System.Threading.Tasks;

namespace Revo.Core.Security
{
    /// <summary>
    /// Service for manual and more fine-grained user action authorization.
    /// </summary>
    public interface IUserPermissionAuthorizer
    {
        /// <summary>
        /// Checks authorization for an action of specified user, requiring specified permission
        /// and (optionally) resource or context.
        /// </summary>
        Task<bool> CheckAuthorizationAsync(IUser user, string permissionId, string resourceId = null, string contextId = null);

        /// <summary>
        /// Checks authorization for an action of the current user (as provided by IUserContext),
        /// requiring specified permission and (optionally) resource or context.
        /// </summary>
        Task<bool> CheckCurrentUserAuthorizationAsync(string permissionId, string resourceId = null, string contextId = null);
    }
}