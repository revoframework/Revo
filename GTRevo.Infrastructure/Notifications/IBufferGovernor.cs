using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Notifications.Model;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IBufferGovernor
    {
        Guid Id { get; }
        //Task AddNotificationAsync(INotification notification);
        Task<MultiValueDictionary<NotificationBuffer, BufferedNotification>> SelectNotificationsForReleaseAsync(IReadRepository readRepository);
    }
}
