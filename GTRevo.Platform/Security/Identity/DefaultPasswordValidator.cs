using Microsoft.AspNet.Identity;

namespace GTRevo.Platform.Security.Identity
{
    public class DefaultPasswordValidator : PasswordValidator, IPasswordValidator
    {
        public DefaultPasswordValidator()
        {
            RequiredLength = 6;
            RequireNonLetterOrDigit = true;
            RequireDigit = true;
            RequireLowercase = true;
            RequireUppercase = true;
        }
    }
}
