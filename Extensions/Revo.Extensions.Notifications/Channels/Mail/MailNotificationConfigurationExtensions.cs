using System;
using Revo.Core.Configuration;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    public static class MailNotificationConfigurationExtensions
    {
        public static IRevoConfiguration ConfigureMailNotifications(this IRevoConfiguration configuration,
            Func<IMailNotificationSender> mailSenderFactory,
            Action<MailNotificationsConfigurationSection> advancedAction = null)
        {
            var section = configuration.GetSection<MailNotificationsConfigurationSection>();
            section.MailSenderFactoryFunc = mailSenderFactory;

            advancedAction?.Invoke(section);

            configuration.ConfigureKernel(c =>
            {
                if (section.MailSenderFactoryFunc != null)
                {
                    c.LoadModule(new MailNotificationsModule(section.MailSenderFactoryFunc));
                }
            });

            return configuration;
        }
    }
}
