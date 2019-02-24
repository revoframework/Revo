using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Revo.Extensions.Notifications
{
    public class NotificationSerializer : INotificationSerializer
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            },
            Formatting = Formatting.None
        };

        static NotificationSerializer()
        {
            JsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

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
                NotificationJson = JsonConvert.SerializeObject(notification, JsonSerializerSettings)
            };

            return serialized;
        }

        public INotification FromJson(SerializedNotification serializedNotification)
        {
            Type notificationType = notificationTypeCache.GetClrNotificationType(serializedNotification.NotificationClassName);
            return (INotification)JsonConvert.DeserializeObject(serializedNotification.NotificationJson, notificationType, JsonSerializerSettings);
        }
    }
}
