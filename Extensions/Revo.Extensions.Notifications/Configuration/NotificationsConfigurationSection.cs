using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Configuration
{
    public class NotificationsConfigurationSection : IRevoConfigurationSection
    {
        public bool IsActive { get; set; }
    }
}