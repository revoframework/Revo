using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Notifications.Channels.Mail
{
    public interface IMailNotificationFormatter
    {
        Task<IEnumerable<SerializableMailMessage>> FormatNotificationMessage(IEnumerable<INotification> notifications);
    }
}
