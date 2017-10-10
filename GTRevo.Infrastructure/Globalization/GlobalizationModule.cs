using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Globalization
{
    public class GlobalizationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILocaleManager, LocaleManager>()
                .To<LocaleManager>()
                .InSingletonScope();

            Bind<LocaleLoader>()
                .ToSelf()
                .InSingletonScope();

            Bind<IMessageRepository>()
                .To<MessageRepository>()
                .InRequestOrJobScope();

            Bind<IApplicationStartListener>()
                .To<LocalizationAppInitializer>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<LocaleLoader>()
                .InSingletonScope();
        }
    }
}
