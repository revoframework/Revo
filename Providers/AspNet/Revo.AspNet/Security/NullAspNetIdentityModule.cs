using System;
using Microsoft.AspNet.Identity;
using Ninject.Modules;
using Revo.AspNet.Security.Identity;
using Revo.Core.Core;

namespace Revo.AspNet.Security
{
    [AutoLoadModule(false)]
    public class NullAspNetIdentityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserStore<IIdentityUser, Guid>>()
                .To<NullUserStore>()
                .InTaskScope();
        }
    }
}
