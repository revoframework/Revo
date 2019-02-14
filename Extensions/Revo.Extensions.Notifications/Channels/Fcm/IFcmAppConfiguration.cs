using PushSharp.Google;

namespace Revo.Extensions.Notifications.Channels.Fcm
{
    public interface IFcmAppConfiguration
    {
        GcmConfiguration FcmConfiguration { get; }
        string AppId { get; }
    }
}
