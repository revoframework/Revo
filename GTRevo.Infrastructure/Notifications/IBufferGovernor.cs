using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
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
