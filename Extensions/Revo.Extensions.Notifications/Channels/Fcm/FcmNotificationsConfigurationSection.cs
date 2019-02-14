using System.Collections.Generic;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public class FcmNotificationsConfigurationSection : IRevoConfigurationSection
    {
        public IReadOnlyCollection<IFcmAppConfiguration> AppConfigurations { get; set; }
    }
}
