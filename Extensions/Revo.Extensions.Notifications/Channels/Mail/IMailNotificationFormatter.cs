using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public interface IMailNotificationFormatter
    {
        Task<IEnumerable<SerializableMailMessage>> FormatNotificationMessage(IReadOnlyCollection<INotification> notifications);
    }
}
