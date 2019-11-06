using System;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Configuration
{
    public static class NotificationsConfigurationExtensions
    {
        public static IRevoConfiguration AddNotificationsExtension(this IRevoConfiguration configuration,
            Action<NotificationsConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<NotificationsConfigurationSection>();
            section.IsActive = true;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsActive)
                {
                    c.LoadModule(new NotificationsModule());
                }
            });

            return configuration;
        }
    }
}