using System;
using Microsoft.AspNet.Identity;
using IUser = Revo.Core.Security.IUser;

namespace Revo.AspNet.Security.Identity
{
    public interface IIdentityUser : IUser<Guid>
    {
        string PasswordHash { get; }
        IUser User { get; }
    }
}
