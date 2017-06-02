using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IBufferedNotificationChannel
    {
        Task SendNotificationsAsync(IEnumerable<INotification> notifications);
    }
}
