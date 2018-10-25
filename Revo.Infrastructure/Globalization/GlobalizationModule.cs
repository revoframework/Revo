using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.Globalization
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
                .InTaskScope();

            Bind<IApplicationStartListener>()
                .To<LocaleLoader>()
                .InSingletonScope();
        }
    }
}
