using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Security;
using Revo.Platforms.AspNet.Security.Identity;

namespace Revo.Platforms.AspNet.Security
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
                    HttpContext.Current?.TryGetOwinContext()?.Authentication);

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
