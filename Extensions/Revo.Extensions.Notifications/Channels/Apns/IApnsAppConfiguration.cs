using PushSharp.Apple;

namespace Revo.Extensions.Notifications.Channels.Apns
{
    public interface IApnsAppConfiguration
    {
        ApnsConfiguration ApnsConfiguration { get; }
        string AppId { get; }
    }
}
