using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface INotificationPipeline
    {
        string Name { get; }
        Task ProcessNotificationsAsync(IReadOnlyCollection<INotification> notifications);
    }
}