using PushSharp.Google;

namespace Revo.Extensions.Notifications.Channels.Fcm
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
