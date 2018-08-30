using System;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Ninject.Modules;
using Revo.AspNet.Security.Identity;
using Revo.Core.Core;
using Revo.Core.Security;

namespace Revo.AspNet.Security
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
