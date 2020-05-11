using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Revo.Core.Security.ClaimBased
{
    public class NullClaimsPrincipalUserResolver : IClaimsPrincipalUserResolver
    {
        public Guid? TryGetUserId(ClaimsPrincipal principal)
        {
            return null;
        }

        public Task<IUser> GetUserAsync(ClaimsPrincipal principal)
        {
            throw new NotImplementedException("Cannot determine authentication using the default NullUserResolver, please use a real implementation.");
        }
    }
}
