using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public class FcmBufferedNotificationChannel : IBufferedNotificationChannel
    {
        private readonly IFcmNotificationFormatter[] pushNotificationFormatters;
        private readonly IFcmBrokerDispatcher fcmBrokerDispatcher;

        public FcmBufferedNotificationChannel(
            IFcmNotificationFormatter[] pushNotificationFormatters,
            IFcmBrokerDispatcher fcmBrokerDispatcher)
        {
            this.pushNotificationFormatters = pushNotificationFormatters;
            this.fcmBrokerDispatcher = fcmBrokerDispatcher;
        }

        public async Task SendNotificationsAsync(IEnumerable<INotification> notifications)
        {
            IEnumerable<GcmNotification> fcmNotifications = null;
            foreach (IFcmNotificationFormatter formatter in pushNotificationFormatters)
            {
                IEnumerable<GcmNotification> pushNotifications = await formatter.FormatPushNotification(notifications);
                fcmNotifications = fcmNotifications?.Concat(pushNotifications) ?? pushNotifications;
            }

            if (fcmNotifications != null)
            {
                fcmBrokerDispatcher.QueueNotifications(fcmNotifications);
            }
        }
    }
}
