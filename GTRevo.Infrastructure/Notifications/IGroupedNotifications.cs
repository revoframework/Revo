using System.Collections.Generic;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IGroupedNotifications : INotification
    {
        IEnumerable<INotification> Notifications { get; }
    }
}
