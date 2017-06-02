using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public class BufferedAdaptorNotificationChannel<T> : INotificationChannel
    {
        private readonly IBufferedNotificationChannel[] bufferedNotificationChannels;

        public BufferedAdaptorNotificationChannel(IBufferedNotificationChannel[] bufferedNotificationChannels)
        {
            this.bufferedNotificationChannels = bufferedNotificationChannels;
        }

        public IEnumerable<Type> NotificationTypes { get; } = new[] { typeof(T) };

        public async Task SendNotificationAsync(INotification notification)
        {
            foreach (IBufferedNotificationChannel channel in bufferedNotificationChannels)
            {
                await channel.SendNotificationsAsync(new[] {notification});
            }
        }
    }
}
