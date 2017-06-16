using Ninject.Modules;

namespace GTRevo.Core
{
    public class CorePlatformModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
                .InTransientScope();
        }
    }
}
