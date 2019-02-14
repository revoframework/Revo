using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferedAdaptorNotificationChannel<T> : INotificationChannel
    {
        private readonly IBufferedNotificationChannel[] bufferedNotificationChannels;

        public BufferedAdaptorNotificationChannel(IBufferedNotificationChannel[] bufferedNotificationChannels)
        {
            this.bufferedNotificationChannels = bufferedNotificationChannels;
        }

        public IEnumerable<Type> NotificationTypes { get; } = new[] { typeof(T) };

        public async Task PushNotificationAsync(INotification notification)
        {
            foreach (IBufferedNotificationChannel channel in bufferedNotificationChannels)
            {
                await channel.SendNotificationsAsync(new[] {notification});
            }
        }

        public Task CommitAsync()
        {
            return Task.FromResult(0);
        }
    }
}
