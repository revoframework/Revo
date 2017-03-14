using System;
using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security.Identity
{
    public class DefaultUserValidator : UserValidator<IUser, Guid>, IUserValidator
    {
        public DefaultUserValidator(AppUserManager userManager) : base(userManager)
        {
            AllowOnlyAlphanumericUserNames = false;
            RequireUniqueEmail = true;
        }
    }
}
