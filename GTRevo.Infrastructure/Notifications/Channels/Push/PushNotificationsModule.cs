using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Platform.Core;
using Ninject;
using Ninject.Modules;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    public class PushNotificationsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApnsBrokerDispatcher, IApplicationStartListener>()
                .To<ApnsBrokerDispatcher>()
                .InSingletonScope();

            Bind<ApnsConfiguration>()
                .ToMethod(ctx =>
                {
                    StandardKernel kernel = (StandardKernel) ctx.Kernel;
                    var config = kernel.Get<IConfiguration>();
                    var configSection =
                        config.GetSection<ApnsServiceConfigurationSection>(ApnsServiceConfigurationSection
                            .ConfigurationSectionName);

                    return configSection != null
                        ? new ApnsConfiguration(configSection.IsSandboxEnvironment
                                ? ApnsConfiguration.ApnsServerEnvironment.Sandbox
                                : ApnsConfiguration.ApnsServerEnvironment.Production,
                            configSection.CertificateFilePath, configSection.CertificatePassword)
                        : null;
                });
        }
    }
}
