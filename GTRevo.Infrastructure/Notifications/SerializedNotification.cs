using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications
{
    public class SerializedNotification
    {
        public string NotificationClassName { get; set; }
        public string NotificationJson { get; set; }
    }
}
