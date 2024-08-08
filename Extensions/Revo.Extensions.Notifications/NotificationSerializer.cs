using System.Text.Json;
using System.Text.Json.Serialization;

namespace Revo.Extensions.Notifications
{
    public class NotificationSerializer(INotificationTypeCache notificationTypeCache) : INotificationSerializer
    {
        private static readonly JsonSerializerOptions JsonSerializerSettings = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        static NotificationSerializer()
        {
            JsonSerializerSettings.Converters.Add(new JsonStringEnumConverter());
        }

        private readonly INotificationTypeCache notificationTypeCache = notificationTypeCache;

        public SerializedNotification ToJson(INotification notification)
        {
            var serialized = new SerializedNotification()
            {
                NotificationClassName = notificationTypeCache.GetNotificationTypeName(notification.GetType()),
                NotificationJson = JsonSerializer.Serialize(notification, notification.GetType(), JsonSerializerSettings)
            };

            return serialized;
        }

        public INotification FromJson(SerializedNotification serializedNotification)
        {
            var notificationType = notificationTypeCache.GetClrNotificationType(serializedNotification.NotificationClassName);
            return (INotification)JsonSerializer.Deserialize(serializedNotification.NotificationJson, notificationType, JsonSerializerSettings);
        }
    }
}
