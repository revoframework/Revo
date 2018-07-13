using System;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
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
            if (!Kernel.GetBindings(typeof(SignInManager<IIdentityUser, Guid>)).Any())
            {
                Bind<SignInManager<IIdentityUser, Guid>>()
                    .To<SignInManager<IIdentityUser, Guid>>()
                    .InRequestOrJobScope();
            }

            if (!Kernel.GetBindings(typeof(UserManager<IIdentityUser, Guid>)).Any())
            {
                Bind<UserManager<IIdentityUser, Guid>>()
                    .To<UserManager<IIdentityUser, Guid>>()
                    .InRequestOrJobScope();
            }

            if (!Kernel.GetBindings(typeof(IUserStore<IIdentityUser, Guid>)).Any())
            {
                Bind<IUserStore<IIdentityUser, Guid>>()
                    .To<NullUserStore>()
                    .InRequestOrJobScope();
            }

            Bind<IAuthenticationManager>()
                .ToMethod(ctx =>
                    HttpContext.Current?.TryGetOwinContext()?.Authentication);

            Bind<IUserContext>()
                .To<AspNetUserContext>()
                .InRequestOrJobScope();

            Bind<IPermissionTypeRegistry>()
                .To<PermissionTypeRegistry>()
                .InSingletonScope();

            Bind<PermissionTypeIndexer, IApplicationStartListener>()
                .To<PermissionTypeIndexer>()
                .InSingletonScope();
            
            Bind<IPermissionAuthorizer>()
                .To<PermissionAuthorizer>()
                .InRequestOrJobScope();

            Bind<IPermissionCache>()
                .To<PermissionCache>()
                .InSingletonScope();
        }
    }
}
