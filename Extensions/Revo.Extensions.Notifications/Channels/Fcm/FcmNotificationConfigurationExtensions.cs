using System;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public static class FcmNotificationConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureFcmNotifications(this IRevoConfiguration configuration,
            IFcmAppConfiguration[] apnsApps = null,
            Action<FcmNotificationsConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<FcmNotificationsConfigurationSection>();
            section.AppConfigurations = apnsApps ?? new IFcmAppConfiguration[0];

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.AppConfigurations.Count > 0)
                {
                    c.LoadModule(new FcmNotificationsModule(section.AppConfigurations));
                }
            });

            return configuration;
        }
    }
}
