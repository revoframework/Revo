using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GTRevo.Platform.Security.Identity;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security
{
    public interface IUser : IUser<Guid>
    {
        string PasswordHash { get; set; }

        Task<ClaimsIdentity> GenerateUserIdentityAsync(AppUserManager manager, string authenticationType);
    }
}
