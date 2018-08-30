using Ninject.Modules;
using Revo.Core.Core;

namespace Revo.AspNetCore.Security.Identity
{
    [AutoLoadModule(false)]
    public class NullAspNetCoreIdentityModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserManager>()
                .To<NullUserManager>()
                .InRequestOrJobScope();

            Bind<ISignInManager>()
                .To<NullSignInManager>()
                .InRequestOrJobScope();
        }
    }
}
