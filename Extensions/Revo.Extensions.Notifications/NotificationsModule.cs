using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Extensions.Notifications.Channels.Mail;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Jobs;

namespace Revo.Extensions.Notifications
{
    [AutoLoadModule(false)]
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
            
            Bind<IApplicationStartedListener>()
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
            
            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(NotificationsModule).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
