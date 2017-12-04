using System.Collections.Generic;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public interface IFcmBrokerDispatcher
    {
        void QueueNotification(WrappedFcmNotification notification);
        void QueueNotifications(IEnumerable<WrappedFcmNotification> notifications);
    }
}
