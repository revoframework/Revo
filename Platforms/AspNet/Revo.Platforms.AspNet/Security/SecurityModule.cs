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
    [AutoLoadModule(false)]
    public class SecurityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<SignInManager<IIdentityUser, Guid>>()
                .To<SignInManager<IIdentityUser, Guid>>()
                .InRequestOrJobScope();

            Bind<UserManager<IIdentityUser, Guid>>()
                .To<UserManager<IIdentityUser, Guid>>()
                .InRequestOrJobScope();

            Bind<IAuthenticationManager>()
                .ToMethod(ctx =>
                    HttpContext.Current?.TryGetOwinContext()?.Authentication);

            Bind<IUserContext>()
                .To<AspNetUserContext>()
                .InRequestOrJobScope();
        }
    }
}
