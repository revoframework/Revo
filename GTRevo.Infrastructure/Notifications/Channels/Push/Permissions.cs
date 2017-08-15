using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.Notifications.Channels.Push
{
    [PermissionTypeCatalog("GTRevo.Infrastructure.Notifications.Channels.Push")]
    public static class Permissions
    {
        public const string RegisterDeviceToken = "{EAA3FA48-1227-4479-969D-D48505335844}";
        public const string DeregisterDeviceToken = "{0F47EDC4-55C4-42E8-8F32-63C9BAE06181}";
        public const string RegisterExternalUserDeviceToken = "{B3362408-C54C-493F-89F2-5E92D11C3FB0}";
        public const string DeregisterExternalUserDeviceToken = "{5C7F8BAE-CBD1-4F81-B782-06C8D4B3B016}";
        public const string PushExternalNotification = "{2A88944A-16F6-494F-8867-33CBD535C1E5}";
    }
}
