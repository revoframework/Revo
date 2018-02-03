using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Revo.Platforms.AspNet.Security.Identity;

namespace Revo.Platforms.AspNet.Security
{
    public interface IIdentityUser : IUser<Guid>
    {
        string PasswordHash { get; set; }

        Task<ClaimsIdentity> GenerateUserIdentityAsync(AppUserManager manager, string authenticationType);
    }
}
