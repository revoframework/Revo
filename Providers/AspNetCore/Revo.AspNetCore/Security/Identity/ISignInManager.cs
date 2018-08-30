using System.Security.Claims;

namespace Revo.AspNetCore.Security.Identity
{
    public interface ISignInManager
    {
        bool IsSignedIn(ClaimsPrincipal principal);
    }
}
