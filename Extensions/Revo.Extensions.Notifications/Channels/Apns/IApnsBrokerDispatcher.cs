using System.Collections.Generic;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public interface IApnsBrokerDispatcher
    {
        void QueueNotification(WrappedApnsNotification notification);
        void QueueNotifications(IEnumerable<WrappedApnsNotification> notifications);
    }
}
