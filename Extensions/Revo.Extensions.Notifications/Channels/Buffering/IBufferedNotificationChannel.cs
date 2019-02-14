using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferedNotificationChannel
    {
        Task SendNotificationsAsync(IReadOnlyCollection<INotification> notifications);
    }
}
