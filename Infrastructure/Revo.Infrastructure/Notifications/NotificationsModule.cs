using System;
using Ninject;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Notifications
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
            
            Bind<IBufferedNotificationStore>()
                .To<BufferedNotificationStore>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                //.To<CrudRepository>()
                .ToMethod(ctx => ctx.Kernel.Get<Func<ICrudRepository>>()()) // TODO probably won't work & needs an explicit factory
                .WhenInjectedInto<BufferedNotificationStore>()
                .InTransientScope();
        }
    }
}
