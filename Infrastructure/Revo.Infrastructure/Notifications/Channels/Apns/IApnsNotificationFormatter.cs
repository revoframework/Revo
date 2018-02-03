using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications.Channels.Apns
{
    public interface IApnsNotificationFormatter
    {
        Task<IEnumerable<WrappedApnsNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
