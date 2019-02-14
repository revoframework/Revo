using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications
{
    public interface INotificationPipeline
    {
        Guid Id { get; }
        Task ProcessNotificationsAsync(IReadOnlyCollection<INotification> notifications);
    }
}