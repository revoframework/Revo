using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Core.Security
{
    /// <summary>
    /// Provides an easier and more efficient way of accessing currently
    /// logged user and his details.
    /// </summary>
    public interface IUserContext
    {
        bool IsAuthenticated { get; }

        /// <summary>
        /// ID of the currently authenticated user for this request (null if no authenticated user).
        /// Note that, in certain situations, this may actually not always give a valid user ID
        /// (e.g. user got deleted in meantime, but his session is still valid). Use with caution
        /// or replace with GetUserAsync().
        /// </summary>
        Guid? UserId { get; }

        Task<IUser> GetUserAsync();
        Task<IReadOnlyCollection<Permission>> GetPermissionsAsync();
    }
}
