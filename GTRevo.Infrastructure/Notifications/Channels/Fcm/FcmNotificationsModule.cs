using GTRevo.Core.Core.Lifecycle;
using GTRevo.Platform.Core;
using Ninject.Modules;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmNotificationsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IFcmBrokerDispatcher, IApplicationStartListener>()
                .To<FcmBrokerDispatcher>()
                .InSingletonScope();

            Bind<GcmConfiguration>()
                .ToMethod(ctx =>
                {
                    var configSection = LocalConfiguration.Current
                        .GetSection<FcmServiceConfigurationSection>(
                            FcmServiceConfigurationSection.ConfigurationSectionName);

                    return configSection?.SenderAuthToken.Length > 0 
                        ? new GcmConfiguration(configSection.SenderAuthToken)
                        : null;
                });
        }
    }
}
