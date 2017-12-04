using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Apple;

namespace GTRevo.Infrastructure.Notifications.Channels.Apns
{
    /// <summary>
    /// Notification specifying which APNS broker to use. 
    /// </summary>
    public class WrappedApnsNotification
    {
        public WrappedApnsNotification(ApnsNotification notification, string appId)
        {
            Notification = notification;
            AppId = appId;
        }

        public ApnsNotification Notification { get; private set; }
        public string AppId { get; private set; }
    }
}
