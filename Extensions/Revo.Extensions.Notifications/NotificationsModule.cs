using System;
using Ninject;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Extensions.Notifications.Channels.Mail;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications
{
    public class NotificationsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<INotificationTypeCache, IApplicationConfigurer>()
                .To<NotificationTypeCache>()
                .InSingletonScope();

            Bind<INotificationSerializer>()
                .To<NotificationSerializer>()
                .InSingletonScope();
            
            Bind<IApplicationStartListener>()
                .To<BufferedNotificationStartup>()
                .InSingletonScope();

            Bind<INotificationBus>()
                .To<NotificationBus>()
                .InTaskScope();

            Bind<IBufferedNotificationStore>()
                .To<BufferedNotificationStore>()
                .InTaskScope();

            Bind<IJobHandler<SendMailNotificationJob>>()
                .To<SendMailNotificationJobHandler>()
                .InTaskScope();

            Bind<IJobHandler<ProcessBufferedNotificationsJob>>()
                .To<ProcessBufferedNotificationsJobHandler>()
                .InTransientScope();
        }
    }
}
