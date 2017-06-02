using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications.Channels.Mail
{
    public class SerializableMailMessage
    {
        public SerializableMailAddress From { get; set; }
        public List<SerializableMailAddress> To { get; set; } = new List<SerializableMailAddress>();
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; } = false;

        public class SerializableMailAddress
        {
            public string Address { get; set; }
            public string DisplayName { get; set; }
        }
    }
}
