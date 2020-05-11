using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Revo.Core.Security.ClaimBased
{
    public interface IClaimsPrincipalUserResolver
    {
        Guid? TryGetUserId(ClaimsPrincipal principal);
        Task<IUser> GetUserAsync(ClaimsPrincipal principal);
    }
}
