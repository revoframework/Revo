using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;

namespace Revo.Extensions.Notifications
{
    public interface IBufferGovernor
    {
        Guid Id { get; }
        //Task AddNotificationAsync(INotification notification);
        Task<MultiValueDictionary<NotificationBuffer, BufferedNotification>> SelectNotificationsForReleaseAsync(IReadRepository readRepository);
    }
}
