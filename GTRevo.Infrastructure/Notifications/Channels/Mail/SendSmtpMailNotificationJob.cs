using System.Net.Mail;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Notifications.Channels.Mail
{
    public class SendSmtpMailNotificationJob
    {
        private readonly SmtpClient smtpClient;

        public SendSmtpMailNotificationJob(SmtpClient smtpClient)
        {
            this.smtpClient = smtpClient;
        }

        public Task Send(SerializableMailMessage message)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(message.From.Address, message.From.DisplayName);

            foreach (var to in message.To)
            {
                mailMessage.To.Add(new MailAddress(to.Address, to.DisplayName));
            }

            mailMessage.Subject = message.Subject;
            mailMessage.Body = message.Body;
            mailMessage.IsBodyHtml = message.IsBodyHtml;

            return smtpClient.SendMailAsync(mailMessage);
        }
    }
}
