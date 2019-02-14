using System;
using Ninject.Modules;
using Revo.Core.Core;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    [AutoLoadModule(false)]
    public class MailNotificationsModule : NinjectModule
    {
        private readonly Func<IMailNotificationSender> mailSenderFactoryFunc;

        public MailNotificationsModule(Func<IMailNotificationSender> mailSenderFactoryFunc)
        {
            this.mailSenderFactoryFunc = mailSenderFactoryFunc;
        }

        public override void Load()
        {
            Bind<IMailNotificationSender>()
                .ToMethod(ctx => mailSenderFactoryFunc())
                .InTaskScope();
        }
    }
}
