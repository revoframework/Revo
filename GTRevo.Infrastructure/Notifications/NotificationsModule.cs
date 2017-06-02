using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.EF6;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Events;
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
