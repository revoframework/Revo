using System.Collections.Generic;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public interface IFcmBrokerDispatcher
    {
        void QueueNotification(GcmNotification notification);
        void QueueNotifications(IEnumerable<GcmNotification> notifications);
    }
}
