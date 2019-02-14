using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications
{
    public class NotificationBus : INotificationBus
    {
        private readonly INotificationChannel[] notificationChannels;

        public NotificationBus(INotificationChannel[] notificationChannels)
        {
            this.notificationChannels = notificationChannels;
        }

        public async Task PushNotificationAsync(INotification notification)
        {
            IEnumerable<INotificationChannel> channels = notificationChannels
                .Where(x => x.NotificationTypes.Any(y => y.IsInstanceOfType(notification)));
            foreach (INotificationChannel channel in channels)
            {
                await channel.PushNotificationAsync(notification);
            }
        }

        public async Task PushNotificationsAsync(IEnumerable<INotification> notifications)
        {
            var notificationsByType = notifications.GroupBy(x => x.GetType());
            
            foreach (var byType in notificationsByType)
            {
                IEnumerable<INotificationChannel> channels = notificationChannels
                      .Where(x => x.NotificationTypes.Any(y => y.IsAssignableFrom(byType.Key)));
                foreach (INotificationChannel channel in channels)
                {
                    foreach (INotification notification in byType)
                    {
                        await channel.PushNotificationAsync(notification);
                    }
                }
            }
        }

        public async Task CommitAsync()
        {
            foreach (var notificationChannel in notificationChannels)
            {
                await notificationChannel.CommitAsync();
            }
        }
    }
}
