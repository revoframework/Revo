using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications
{
    public interface INotificationChannel
    {
        IEnumerable<Type> NotificationTypes { get; }
        Task PushNotificationAsync(INotification notification);
        Task CommitAsync();
    }
}
