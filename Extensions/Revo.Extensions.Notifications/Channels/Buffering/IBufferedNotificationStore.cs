using System;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferedNotificationStore
    {
        Task Add(SerializedNotification serializedNotification, Guid bufferId,
            DateTimeOffset timeQueued, Guid bufferGovernorId, Guid notificationPipelineId);

        Task CommitAsync();
    }
}
