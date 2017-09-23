using System;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security.Identity
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
