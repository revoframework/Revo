using System;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public class MailNotificationsConfigurationSection : IRevoConfigurationSection
    {
        public Func<IMailNotificationSender> MailSenderFactoryFunc { get; set; }
    }
}
