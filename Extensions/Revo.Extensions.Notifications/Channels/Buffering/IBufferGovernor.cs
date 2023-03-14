using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public interface IBufferGovernor
    {
        string Name { get; }
        //Task AddNotificationAsync(INotification notification);
        Task<MultiValueDictionary<NotificationBuffer, BufferedNotification>> SelectNotificationsForReleaseAsync(IReadRepository readRepository);
    }
}
