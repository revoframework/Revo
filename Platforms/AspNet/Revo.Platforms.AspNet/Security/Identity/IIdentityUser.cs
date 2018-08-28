using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using IUser = Revo.Core.Security.IUser;

namespace Revo.Platforms.AspNet.Security.Identity
{
    public interface IIdentityUser : IUser<Guid>
    {
        string PasswordHash { get; }
        IUser User { get; }
    }
}
