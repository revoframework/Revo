using System.Collections.Generic;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    [AutoLoadModule(false)]
    public class ApnsNotificationsModule : NinjectModule
    {
        private readonly IReadOnlyCollection<IApnsAppConfiguration> appConfigurations;

        public ApnsNotificationsModule(IReadOnlyCollection<IApnsAppConfiguration> appConfigurations)
        {
            this.appConfigurations = appConfigurations;
        }

        public override void Load()
        {
            Bind<IApnsBrokerDispatcher, IApplicationStartListener>()
                .ToMethod(ctx => new ApnsBrokerDispatcher(appConfigurations))
                .InSingletonScope();
        }
    }
}
