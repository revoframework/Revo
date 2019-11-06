using System;
using Revo.Core.Configuration;
using Revo.Extensions.History.ChangeTracking;

namespace Revo.Extensions.History.Configuration
{
    public static class NotificationsConfigurationExtensions
    {
        public static IRevoConfiguration AddHistoryExtension(this IRevoConfiguration configuration,
            bool? isChangeTrackingActive = true,
            Action<HistoryConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<HistoryConfigurationSection>();
            section.IsChangeTrackingActive = isChangeTrackingActive ?? section.IsChangeTrackingActive;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.IsChangeTrackingActive)
                {
                    c.LoadModule(new HistoryModule());
                }
            });

            return configuration;
        }
    }
}