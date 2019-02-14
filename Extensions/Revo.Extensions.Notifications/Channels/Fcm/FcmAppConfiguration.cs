using PushSharp.Google;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public class FcmAppConfiguration : IFcmAppConfiguration
    {
        public string AppId { get; set; }
        public GcmConfiguration FcmConfiguration { get; set; }
    }
}
