using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.AspNet.Globalization
{
    [AutoLoadModule(false)]
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
