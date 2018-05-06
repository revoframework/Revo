using System.Collections.Generic;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public interface IFcmBrokerDispatcher
    {
        void QueueNotification(WrappedFcmNotification notification);
        void QueueNotifications(IEnumerable<WrappedFcmNotification> notifications);
    }
}
