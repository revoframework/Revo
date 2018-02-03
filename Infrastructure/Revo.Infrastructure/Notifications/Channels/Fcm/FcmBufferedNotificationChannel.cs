using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications.Channels.Fcm
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
            IEnumerable<WrappedFcmNotification> fcmNotifications = null;
            foreach (IFcmNotificationFormatter formatter in pushNotificationFormatters)
            {
                IEnumerable<WrappedFcmNotification> pushNotifications = await formatter.FormatPushNotification(notifications);
                fcmNotifications = fcmNotifications?.Concat(pushNotifications) ?? pushNotifications;
            }

            if (fcmNotifications != null)
            {
                fcmBrokerDispatcher.QueueNotifications(fcmNotifications);
            }
        }
    }
}
