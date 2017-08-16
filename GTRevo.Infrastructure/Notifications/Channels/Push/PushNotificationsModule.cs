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
                    IKernel kernel = ctx.Kernel;
                    var config = kernel.Get<IConfiguration>();
                    var configSection = LocalConfiguration.Current
                        .GetSection<ApnsServiceConfigurationSection>(
                            ApnsServiceConfigurationSection.ConfigurationSectionName);

                    return configSection?.CertificateFilePath.Length > 0 
                        ? new ApnsConfiguration(configSection.IsSandboxEnvironment
                                ? ApnsConfiguration.ApnsServerEnvironment.Sandbox
                                : ApnsConfiguration.ApnsServerEnvironment.Production,
                            configSection.CertificateFilePath, configSection.CertificatePassword)
                        : null;
                });
        }
    }
}
