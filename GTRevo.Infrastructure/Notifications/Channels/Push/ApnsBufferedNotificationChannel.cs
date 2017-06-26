using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    public class ApnsBufferedNotificationChannel : IBufferedNotificationChannel
    {
        private readonly IApnsNotificationFormatter[] pushNotificationFormatters;
        private readonly ApnsServiceBroker apnsServiceBroker;

        public ApnsBufferedNotificationChannel(
            IApnsNotificationFormatter[] pushNotificationFormatters,
            ApnsServiceBroker apnsServiceBroker)
        {
            this.pushNotificationFormatters = pushNotificationFormatters;
            this.apnsServiceBroker = apnsServiceBroker;
        }

        public async Task SendNotificationsAsync(IEnumerable<INotification> notifications)
        {
            foreach (IApnsNotificationFormatter formatter in pushNotificationFormatters)
            {
                IEnumerable<ApnsNotification> pushNotifications = await formatter.FormatPushNotification(notifications);
                foreach (ApnsNotification pushNotification in pushNotifications)
                {
                    lock (apnsServiceBroker)
                    {
                        apnsServiceBroker.QueueNotification(pushNotification);
                    }
                }
            }
        }
    }
}
