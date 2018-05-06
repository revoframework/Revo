using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public interface IApnsNotificationFormatter
    {
        Task<IEnumerable<WrappedApnsNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
