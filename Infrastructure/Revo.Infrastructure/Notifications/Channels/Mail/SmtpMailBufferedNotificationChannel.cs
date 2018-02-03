using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;

namespace Revo.Infrastructure.Notifications.Channels.Mail
{
    public class SmtpMailBufferedNotificationChannel : IBufferedNotificationChannel
    {
        private readonly IMailNotificationFormatter[] mailNotificationFormatters;

        public SmtpMailBufferedNotificationChannel(IMailNotificationFormatter[] mailNotificationFormatters)
        {
            this.mailNotificationFormatters = mailNotificationFormatters;
        }

        public async Task SendNotificationsAsync(IEnumerable<INotification> notifications)
        {
            foreach (IMailNotificationFormatter formatter in mailNotificationFormatters)
            {
                IEnumerable<SerializableMailMessage> messages = await formatter.FormatNotificationMessage(notifications);
                foreach (SerializableMailMessage message in messages)
                {
                    BackgroundJob.Enqueue<SendSmtpMailNotificationJob>(job => job.Send(message));
                }
            }
        }
    }
}
