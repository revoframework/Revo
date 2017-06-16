using System;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IBufferedNotificationStore
    {
        Task Add(SerializedNotification serializedNotification, Guid bufferId,
            DateTime timeQueued, Guid bufferGovernorId, Guid notificationPipelineId);
    }
}
