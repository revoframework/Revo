using Microsoft.AspNet.Identity;

namespace Revo.Platforms.AspNet.Security.Identity
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
