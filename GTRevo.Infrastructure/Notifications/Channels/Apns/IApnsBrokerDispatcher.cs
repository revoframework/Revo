using System.Collections.Generic;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    public interface IApnsBrokerDispatcher
    {
        void QueueNotification(WrappedApnsNotification notification);
        void QueueNotifications(IEnumerable<WrappedApnsNotification> notifications);
    }
}
