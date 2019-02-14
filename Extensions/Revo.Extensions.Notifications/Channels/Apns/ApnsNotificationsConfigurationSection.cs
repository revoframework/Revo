using System.Collections.Generic;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public class ApnsNotificationsConfigurationSection : IRevoConfigurationSection
    {
        public IReadOnlyCollection<IApnsAppConfiguration> AppConfigurations { get; set; }
    }
}
