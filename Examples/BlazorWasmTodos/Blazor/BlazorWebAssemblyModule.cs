using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Security;
using IConfiguration = Revo.Core.Core.IConfiguration;

namespace Revo.Examples.BlazorWasmTodos.Blazor
{
    public class BlazorWebAssemblyModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IConfiguration>()
                .ToMethod(ctx => LocalConfiguration.Current)
                .InTransientScope();

            Bind<IUserContext>()
	            .To<BlazorUserContext>()
	            .InSingletonScope();

            Bind<IActorContext>()
                .To<BlazorActorContext>()
                .InSingletonScope();

            Bind<IServiceLocator>()
                .To<NinjectServiceLocator>()
                .InSingletonScope();

            Bind<IEnvironmentProvider>()
                .To<BlazorEnvironmentProvider>()
                .InSingletonScope();
        }
    }
}
