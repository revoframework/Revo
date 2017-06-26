using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    public interface IApnsNotificationFormatter
    {
        Task<IEnumerable<ApnsNotification>> FormatPushNotification(IEnumerable<INotification> notifications);
    }
}
