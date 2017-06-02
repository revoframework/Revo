using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public interface INotificationBus
    {
        Task PushNotification(INotification notification);
        Task PushNotifications(IEnumerable<INotification> notifications);
    }
}