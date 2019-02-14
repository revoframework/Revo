using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications
{
    public interface INotificationBus
    {
        Task PushNotificationAsync(INotification notification);
        Task PushNotificationsAsync(IEnumerable<INotification> notifications);
        Task CommitAsync();
    }
}