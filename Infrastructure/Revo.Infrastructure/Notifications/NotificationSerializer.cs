using System;
using Newtonsoft.Json;

namespace Revo.Infrastructure.Notifications
{
    public class NotificationSerializer : INotificationSerializer
    {
        private readonly INotificationTypeCache notificationTypeCache;

        public NotificationSerializer(INotificationTypeCache notificationTypeCache)
        {
            this.notificationTypeCache = notificationTypeCache;
        }

        public SerializedNotification ToJson(INotification notification)
        {
            SerializedNotification serialized = new SerializedNotification()
            {
                NotificationClassName = notificationTypeCache.GetNotificationTypeName(notification.GetType()),
                NotificationJson = JsonConvert.SerializeObject(notification)
            };

            return serialized;
        }

        public INotification FromJson(SerializedNotification serializedNotification)
        {
            Type notificationType = notificationTypeCache.GetClrNotificationType(serializedNotification.NotificationClassName);
            return (INotification)JsonConvert.DeserializeObject(serializedNotification.NotificationJson, notificationType);
        }
    }
}
