using System;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferedNotificationStore
    {
        Task Add(SerializedNotification serializedNotification, string bufferName,
            DateTimeOffset timeQueued, string bufferGovernorName, string notificationPipelineName);

        Task CommitAsync();
    }
}
