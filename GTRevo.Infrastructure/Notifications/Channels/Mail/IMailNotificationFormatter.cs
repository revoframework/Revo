using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications.Channels.Mail
{
    public interface IMailNotificationFormatter
    {
        Task<IEnumerable<SerializableMailMessage>> FormatNotificationMessage(IEnumerable<INotification> notifications);
    }
}
