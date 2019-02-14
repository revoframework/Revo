using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class MailBufferedNotificationChannel : IBufferedNotificationChannel
    {
        private readonly IMailNotificationFormatter[] mailNotificationFormatters;
        private readonly IJobScheduler jobScheduler;

        public MailBufferedNotificationChannel(IMailNotificationFormatter[] mailNotificationFormatters,
            IJobScheduler jobScheduler)
        {
            this.mailNotificationFormatters = mailNotificationFormatters;
            this.jobScheduler = jobScheduler;
        }

        public async Task SendNotificationsAsync(IReadOnlyCollection<INotification> notifications)
        {
            foreach (IMailNotificationFormatter formatter in mailNotificationFormatters)
            {
                IEnumerable<SerializableMailMessage> messages = await formatter.FormatNotificationMessage(notifications);
                foreach (SerializableMailMessage message in messages)
                {
                    await jobScheduler.EnqeueJobAsync(new SendMailNotificationJob(message));
                }
            }
        }
    }
}
