using System;
using System.Security.Claims;

namespace Revo.AspNetCore.Security.Identity
{
    public class NullSignInManager : ISignInManager
    {
        public bool IsSignedIn(ClaimsPrincipal principal)
        {
            throw new NotImplementedException("Cannot determine authentication using the default NullSignInManager, please use a real implemention.");
        }
    }
}
