using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class SendMailNotificationJob : IJob
    {
        public SendMailNotificationJob(SerializableMailMessage message)
        {
            Message = message;
        }

        public SerializableMailMessage Message { get; }
    }
}
