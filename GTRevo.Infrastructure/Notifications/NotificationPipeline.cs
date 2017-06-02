using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public class NotificationPipeline : INotificationPipeline
    {
        private readonly IBufferedNotificationChannel[] bufferedNotificationChannels;

        public NotificationPipeline(Guid id,
            IBufferedNotificationChannel[] bufferedNotificationChannels)
        {
            Id = id;
            this.bufferedNotificationChannels = bufferedNotificationChannels;
        }

        public Guid Id { get; }

        public async Task ProcessNotificationsAsync(IEnumerable<INotification> notifications)
        {
            foreach (IBufferedNotificationChannel channel in bufferedNotificationChannels)
            {
                await channel.SendNotificationsAsync(notifications);
            }
        }
    }
}
