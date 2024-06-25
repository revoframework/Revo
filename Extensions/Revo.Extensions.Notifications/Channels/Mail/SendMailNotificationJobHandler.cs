using System.Threading;
using System.Threading.Tasks;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class SendMailNotificationJobHandler(IMailNotificationSender mailNotificationSender) 
        : IJobHandler<SendMailNotificationJob>
    {
        public Task HandleAsync(SendMailNotificationJob job, CancellationToken cancellationToken)
        {
            return mailNotificationSender.SendMessages(new[] {job.Message});
        }
    }
}
