using PushSharp.Apple;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public class ApnsAppConfiguration : IApnsAppConfiguration
    {
        public string AppId { get; set; }
        public ApnsConfiguration ApnsConfiguration { get; set; }
    }
}
