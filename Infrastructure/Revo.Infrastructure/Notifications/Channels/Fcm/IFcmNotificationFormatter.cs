using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications.Channels.Fcm
{
    public interface IFcmNotificationFormatter
    {
        Task<IEnumerable<WrappedFcmNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
