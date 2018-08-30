using System.Security.Claims;
using System.Threading.Tasks;
using Revo.Core.Security;

namespace Revo.AspNetCore.Security.Identity
{
    public interface IUserManager
    {
        string GetUserId(ClaimsPrincipal principal);
        Task<IUser> GetUserAsync(ClaimsPrincipal principal);
    }
}
