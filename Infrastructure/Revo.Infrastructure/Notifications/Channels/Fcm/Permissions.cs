using Revo.Core.Security;

namespace Revo.Infrastructure.Notifications.Channels.Fcm
{
    [PermissionTypeCatalog("Revo.Infrastructure.Notifications.Channels.Fcm")]
    public static class Permissions
    {
        public const string RegisterDeviceToken = "{2D3688FC-38AF-40AA-AF31-F5DA3BCD6AE3}";
        public const string DeregisterDeviceToken = "{DAC86A0D-3FE4-4FE2-AD4E-D330D97A00FC}";
        public const string RegisterExternalUserDeviceToken = "{69E47373-A6E7-4FBA-9EB4-3934BBC7DC49}";
        public const string DeregisterExternalUserDeviceToken = "{47067382-04EE-45A2-85D9-55553B034216}";
        public const string PushExternalNotification = "{9A791006-1EDD-4C68-A685-85BFBEA22598}";
    }
}
