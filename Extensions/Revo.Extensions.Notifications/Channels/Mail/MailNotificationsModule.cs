using System;
using Ninject.Modules;
using Revo.Core.Core;

namespace Revo.Extensions.Notifications.Channels.Mail
{
    [AutoLoadModule(false)]
    public class MailNotificationsModule(Func<IMailNotificationSender> mailSenderFactoryFunc) : NinjectModule
    {
        public override void Load()
        {
            Bind<IMailNotificationSender>()
                .ToMethod(ctx => mailSenderFactoryFunc())
                .InTaskScope();
        }
    }
}
