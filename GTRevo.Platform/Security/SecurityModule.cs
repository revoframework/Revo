using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Security.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject.Modules;

namespace GTRevo.Platform.Security
{
    public class SecurityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<AppUserManager>()
                .To<AppUserManager>()
                .InRequestOrJobScope();

            Bind<AppSignInManager/*, SignInManager<IUser, Guid>*/>()
                .To<AppSignInManager>()
                .InRequestOrJobScope();

            Bind<IAuthenticationManager>()
                .ToMethod(ctx =>
                    HttpContext.Current.GetOwinContext().Authentication);

            Bind<IUserContext>()
                .To<DefaultUserContext>()
                .InRequestOrJobScope();

            Bind<IPermissionTypeRegistry>()
                .To<PermissionTypeRegistry>()
                .InSingletonScope();

            Bind<PermissionTypeIndexer, IApplicationStartListener>()
                .To<PermissionTypeIndexer>()
                .InSingletonScope();

            Bind<IPasswordHasher>()
                .To<ScryptPasswordHasher>()
                .InSingletonScope();

            Bind<IPasswordValidator>()
                .To<DefaultPasswordValidator>()
                .InRequestOrJobScope();

            Bind<IUserValidator>()
                .To<DefaultUserValidator>()
                .InRequestOrJobScope();

            Bind<PermissionAuthorizer>()
                .ToSelf()
                .InRequestOrJobScope();

            Bind<PermissionCache>()
                .ToSelf()
                .InSingletonScope();
        }
    }
}
