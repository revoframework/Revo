using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class SmtpMailNotificationSender : IMailNotificationSender
    {
        private readonly SmtpClient smtpClient;

        public SmtpMailNotificationSender(SmtpClient smtpClient)
        {
            this.smtpClient = smtpClient;
        }

        public async Task SendMessages(IReadOnlyCollection<SerializableMailMessage> messages)
        {
            foreach (var message in messages)
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

                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
}
