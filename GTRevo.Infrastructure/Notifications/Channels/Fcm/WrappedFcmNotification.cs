using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PushSharp.Google;

namespace GTRevo.Infrastructure.Notifications.Channels.Fcm
{
    /// <summary>
    /// Notification specifying which FCM broker to use. 
    /// </summary>
    public class WrappedFcmNotification
    {
        public WrappedFcmNotification(GcmNotification notification, string appId)
        {
            Notification = notification;
            AppId = appId;
        }

        public GcmNotification Notification { get; private set; }
        public string AppId { get; private set; }
    }
}
