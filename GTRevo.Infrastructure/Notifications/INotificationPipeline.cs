using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public interface INotificationPipeline
    {
        Guid Id { get; }

        Task ProcessNotificationsAsync(IEnumerable<INotification> notifications);
    }
}