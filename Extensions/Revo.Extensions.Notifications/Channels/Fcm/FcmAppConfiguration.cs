using PushSharp.Google;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public class FcmAppConfiguration
    {
        public FcmAppConfiguration(string appId, GcmConfiguration apnsConfiguration)
        {
            AppId = appId;
            FcmConfiguration = apnsConfiguration;
        }

        public string AppId { get; private set; }
        public GcmConfiguration FcmConfiguration { get; private set; }
    }
}
