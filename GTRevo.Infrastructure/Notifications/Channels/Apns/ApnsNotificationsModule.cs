using System.Collections.Generic;
using System.Linq;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Platform.Core;
using Ninject.Modules;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    public class ApnsNotificationsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApnsBrokerDispatcher, IApplicationStartListener>()
                .ToMethod(ctx =>
                {
                    var configSection = LocalConfiguration.Current
                        .GetSection<ApnsServiceConfigurationSection>(
                            ApnsServiceConfigurationSection.ConfigurationSectionName);

                    List<ApnsAppConfiguration> configs = new List<ApnsAppConfiguration>();

                    if (configSection != null)
                    {
                        for (int i = 0; i < configSection.AppConfigurations.Count; i++)
                        {
                            var configElement = configSection.AppConfigurations[i];
                            configs.Add(new ApnsAppConfiguration(configElement.AppId,
                                new ApnsConfiguration(configElement.IsSandboxEnvironment
                                        ? ApnsConfiguration.ApnsServerEnvironment.Sandbox
                                        : ApnsConfiguration.ApnsServerEnvironment.Production,
                                    configElement.CertificateFilePath, configElement.CertificatePassword)));
                        }
                    }

                    return new ApnsBrokerDispatcher(configs);
                })
                .InSingletonScope();
        }
    }
}
