using Ninject.Modules;
using Revo.AspNetCore.Core;
using Revo.AspNetCore.Ninject;
using Revo.Core.Core;

namespace Revo.AspNetCore
{
    [AutoLoadModule(false)]
    public class AspNetCoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConfiguration>()
                .ToMethod(ctx => LocalConfiguration.Current)
                .InTransientScope();

            Bind<IActorContext>()
                .To<UserActorContext>()
                .InRequestOrJobScope();

            Bind<IServiceLocator>()
                .To<NinjectServiceLocator>()
                .InSingletonScope();
        }
    }
}
