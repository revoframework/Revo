using System.Collections.Generic;

namespace Revo.Extensions.Notifications
{
    public interface IGroupedNotifications : INotification
    {
        IEnumerable<INotification> Notifications { get; }
    }
}
