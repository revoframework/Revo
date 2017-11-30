using System.Collections.Generic;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    public interface IApnsNotificationFormatter
    {
        Task<IEnumerable<ApnsNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
