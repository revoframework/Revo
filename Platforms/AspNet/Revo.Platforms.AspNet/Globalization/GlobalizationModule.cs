using Ninject.Modules;
using Revo.Core.Lifecycle;

namespace Revo.Platforms.AspNet.Globalization
{
    public class GlobalizationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartListener>()
                .To<LocalizationAppInitializer>()
                .InSingletonScope();
        }
    }
}
