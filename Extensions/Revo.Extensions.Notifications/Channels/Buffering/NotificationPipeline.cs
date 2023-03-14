using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class NotificationPipeline : INotificationPipeline
    {
        private readonly IBufferedNotificationChannel[] bufferedNotificationChannels;

        public NotificationPipeline(string name,
            IBufferedNotificationChannel[] bufferedNotificationChannels)
        {
            Name = name;
            this.bufferedNotificationChannels = bufferedNotificationChannels;
        }

        public string Name { get; }

        public async Task ProcessNotificationsAsync(IReadOnlyCollection<INotification> notifications)
        {
            foreach (IBufferedNotificationChannel channel in bufferedNotificationChannels)
            {
                await channel.SendNotificationsAsync(notifications);
            }
        }
    }
}
