using PushSharp.Apple;

namespace Revo.Extensions.Notifications.Channels.Apns
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
