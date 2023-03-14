using Ninject.Modules;

namespace Revo.Core.Lifecycle
{
    public class LifecycleModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationConfigurerInitializer>()
                .To<ApplicationConfigurerInitializer>()
                .InSingletonScope();

            Bind<IApplicationLifecycleNotifier>()
                .To<ApplicationLifecycleNotifier>()
                .InSingletonScope();
        }
    }
}
