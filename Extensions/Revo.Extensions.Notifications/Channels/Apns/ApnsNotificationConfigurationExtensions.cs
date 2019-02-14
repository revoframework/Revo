using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revo.Core.Configuration;
using Revo.Infrastructure;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs.InMemory;
using Revo.Infrastructure.Sagas;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public static class ApnsNotificationConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureApnsNotifications(this IRevoConfiguration configuration,
            IApnsAppConfiguration[] apnsApps = null,
            Action<ApnsNotificationsConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<ApnsNotificationsConfigurationSection>();
            section.AppConfigurations = apnsApps ?? new IApnsAppConfiguration[0];

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.AppConfigurations.Count > 0)
                {
                    c.LoadModule(new ApnsNotificationsModule(section.AppConfigurations));
                }
            });

            return configuration;
        }
    }
}
