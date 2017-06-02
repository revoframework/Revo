using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Notifications.Model;
using GTRevo.Platform.Events;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IBufferedNotificationStore
    {
        Task Add(SerializedNotification serializedNotification, Guid bufferId,
            DateTime timeQueued, Guid bufferGovernorId, Guid notificationPipelineId);
    }
}
