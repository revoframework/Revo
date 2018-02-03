using System.Collections.Generic;

namespace Revo.Infrastructure.Notifications
{
    public interface IGroupedNotifications : INotification
    {
        IEnumerable<INotification> Notifications { get; }
    }
}
