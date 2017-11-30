using System.Collections.Generic;
using System.Threading.Tasks;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    public interface IFcmNotificationFormatter
    {
        Task<IEnumerable<GcmNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
