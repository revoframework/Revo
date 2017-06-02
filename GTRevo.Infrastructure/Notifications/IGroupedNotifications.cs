using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public interface IGroupedNotifications : INotification
    {
        IEnumerable<INotification> Notifications { get; }
    }
}
