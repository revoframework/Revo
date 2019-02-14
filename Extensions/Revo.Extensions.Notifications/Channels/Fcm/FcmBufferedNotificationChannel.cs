using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Extensions.Notifications.Channels.Buffering;

namespace Revo.Extensions.Notifications.Channels.Fcm
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

        public async Task SendNotificationsAsync(IReadOnlyCollection<INotification> notifications)
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
