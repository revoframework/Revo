using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Platforms.AspNet.Security.Identity;

namespace Revo.Platforms.AspNet.Security
{
    [AutoLoadModule(false)]
    public class NullAspNetIdentityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserStore<IIdentityUser, Guid>>()
                .To<NullUserStore>()
                .InRequestOrJobScope();
        }
    }
}
