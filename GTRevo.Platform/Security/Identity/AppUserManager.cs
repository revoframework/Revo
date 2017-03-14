using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using Ninject;

namespace GTRevo.Platform.Security.Identity
{
    public class AppUserManager : UserManager<IUser, Guid>, IInitializable
    {
        private readonly Func<AppUserManager, IPasswordValidator> passwordValidatorFunc;
        private readonly Func<AppUserManager, IUserValidator> userValidatorFunc;
        private readonly IPermissionTypeRegistry permissionTypeRegistry;

        public AppUserManager(IAppUserStore userStore,
            IDataProtectionProvider dataProtectionProvider,
            IPermissionTypeRegistry permissionTypeRegistry,
            Func<AppUserManager, IUserValidator> userValidatorFunc,
            Func<AppUserManager, IPasswordValidator> passwordValidatorFunc,
            IPasswordHasher passwordHasher)
            : base(userStore)
        {
            this.permissionTypeRegistry = permissionTypeRegistry;
            this.PasswordHasher = passwordHasher;
            this.ClaimsIdentityFactory = new AppClaimsIdentityFactory();
            this.userValidatorFunc = userValidatorFunc;
            this.passwordValidatorFunc = passwordValidatorFunc;

            // Configure user lockout defaults
            /*manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
            {
                MessageFormat = "Your security code is {0}"
            });
            manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
            {
                Subject = "Security Code",
                BodyFormat = "Your security code is {0}"
            });

            manager.EmailService = new EmailService();
            manager.SmsService = new SmsService();*/

            if (dataProtectionProvider != null)
            {
                this.UserTokenProvider =
                    new DataProtectorTokenProvider<IUser, Guid>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
        }

        public virtual async Task<IEnumerable<Permission>> GetUserPermissionsAsync(IUser user)
        {
            var rolePermissions = await ((IAppUserStore)Store).GetUserPermissionsAsync(user);
            return rolePermissions.Select(
                x => new Permission()
                {
                    PermissionType = permissionTypeRegistry.GetPermissionTypeById(x.PermissionTypeId),
                    ResourceId = x.ResourceId,
                    ContextId = x.ContextId
                });
        }

        public void Initialize()
        {
            UserValidator = userValidatorFunc(this);
            PasswordValidator = passwordValidatorFunc(this);
        }
    }
}
