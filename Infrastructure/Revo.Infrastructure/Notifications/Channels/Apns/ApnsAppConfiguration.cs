using PushSharp.Apple;

namespace Revo.Infrastructure.Notifications.Channels.Apns
{
    public class ApnsAppConfiguration
    {
        public ApnsAppConfiguration(string appId, ApnsConfiguration apnsConfiguration)
        {
            AppId = appId;
            ApnsConfiguration = apnsConfiguration;
        }

        public string AppId { get; private set; }
        public ApnsConfiguration ApnsConfiguration { get; private set; }
    }
}
