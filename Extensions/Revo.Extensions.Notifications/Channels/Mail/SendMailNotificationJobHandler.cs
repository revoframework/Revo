using System.Threading;
using System.Threading.Tasks;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class SendMailNotificationJobHandler : IJobHandler<SendMailNotificationJob>
    {
        private readonly IMailNotificationSender mailNotificationSender;

        public SendMailNotificationJobHandler(IMailNotificationSender mailNotificationSender)
        {
            this.mailNotificationSender = mailNotificationSender;
        }

        public Task HandleAsync(SendMailNotificationJob job, CancellationToken cancellationToken)
        {
            return mailNotificationSender.SendMessages(new[] {job.Message});
        }
    }
}
