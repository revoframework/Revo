using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class SendMailNotificationJob(SerializableMailMessage message) : IJob
    {
        public SerializableMailMessage Message { get; } = message;
    }
}
