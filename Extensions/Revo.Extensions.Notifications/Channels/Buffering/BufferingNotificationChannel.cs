﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferingNotificationChannel<T>(
            IBufferGovernor bufferGovernor,
            IBufferSelector<T> bufferSelector,
            INotificationPipeline notificationPipeline,
            INotificationSerializer notificationSerializer,
            IBufferedNotificationStore bufferedNotificationStore) : INotificationChannel
        where T : class, INotification
    {
        public IEnumerable<Type> NotificationTypes { get; } = new[] { typeof(T) };

        public async Task PushNotificationAsync(INotification notification)
        {
            await AddNotification(notification);
        }

        public Task CommitAsync()
        {
            return bufferedNotificationStore.CommitAsync();
        }

        private async Task AddNotification(INotification notification)
        {
            T tNotification = notification as T;
            if (tNotification == null)
            {
                throw new ArgumentException(
                    $"Invalid notification passed to {this.GetType().FullName}.PushNotificationAsync: {notification.GetType().FullName}");
            }

            SerializedNotification serialized = notificationSerializer.ToJson(notification);

            var bufferId = await bufferSelector.SelectBufferIdAsync(tNotification);

            await bufferedNotificationStore.Add(serialized, bufferId, Clock.Current.UtcNow,
                bufferGovernor.Name, notificationPipeline.Name);
        }
    }
}
