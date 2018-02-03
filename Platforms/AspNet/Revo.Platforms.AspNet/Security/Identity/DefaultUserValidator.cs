using System;
using Microsoft.AspNet.Identity;

namespace Revo.Platforms.AspNet.Security.Identity
{
    public class DefaultUserValidator : UserValidator<IIdentityUser, Guid>, IUserValidator
    {
        public DefaultUserValidator(AppUserManager userManager) : base(userManager)
        {
            AllowOnlyAlphanumericUserNames = false;
            RequireUniqueEmail = true;
        }
    }
}
