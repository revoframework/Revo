using GTRevo.Core.Lifecycle;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Events;
using GTRevo.Platform.Core;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Notifications
{
    public class NotificationsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<INotificationTypeCache, IApplicationStartListener>()
                .To<NotificationTypeCache>()
                .InSingletonScope();

            Bind<INotificationSerializer>()
                .To<NotificationSerializer>()
                .InSingletonScope();

            Bind<INotificationBus>()
                .To<NotificationBus>()
                .InSingletonScope();

            Bind<BufferedNotificationDaemon, IApplicationStartListener>()
                .To<BufferedNotificationDaemon>()
                .InSingletonScope();

            Bind<BufferedNotificationProcessJob>()
                .ToSelf()
                .InTransientScope();
            
            Bind<IBufferedNotificationStore, IEventQueueTransactionListener>()
                .To<BufferedNotificationStore>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                .To<CrudRepository>()
                .WhenInjectedInto<BufferedNotificationStore>()
                .InTransientScope();
        }
    }
}
