using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Revo.Core.Security;

namespace Revo.AspNetCore.Security.Identity
{
    public class NullUserManager : IUserManager
    {
        public string GetUserId(ClaimsPrincipal principal)
        {
            throw new NotImplementedException("Cannot determine authentication using the default NullUserManager, please use a real implemention.");
        }

        public Task<IUser> GetUserAsync(ClaimsPrincipal principal)
        {
            throw new NotImplementedException("Cannot determine authentication using the default NullUserManager, please use a real implemention.");
        }
    }
}
