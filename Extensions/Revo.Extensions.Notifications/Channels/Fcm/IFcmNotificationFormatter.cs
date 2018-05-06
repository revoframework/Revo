using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public interface IFcmNotificationFormatter
    {
        Task<IEnumerable<WrappedFcmNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
