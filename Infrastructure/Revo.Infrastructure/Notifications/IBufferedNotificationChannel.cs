using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications
{
    public interface IBufferedNotificationChannel
    {
        Task SendNotificationsAsync(IEnumerable<INotification> notifications);
    }
}
