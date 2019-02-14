using System.Collections.Generic;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    [AutoLoadModule(false)]
    public class FcmNotificationsModule : NinjectModule
    {
        private readonly IReadOnlyCollection<IFcmAppConfiguration> appConfigurations;

        public FcmNotificationsModule(IReadOnlyCollection<IFcmAppConfiguration> appConfigurations)
        {
            this.appConfigurations = appConfigurations;
        }

        public override void Load()
        {
            Bind<IFcmBrokerDispatcher, IApplicationStartListener>()
                .ToMethod(ctx => new FcmBrokerDispatcher(appConfigurations))
                .InSingletonScope();
        }
    }
}
