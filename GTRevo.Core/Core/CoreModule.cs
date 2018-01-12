using GTRevo.Core.Commands;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using Ninject;
using Ninject.Modules;

namespace GTRevo.Core.Core
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
                .InTransientScope();

            Bind<ICommandBus>()
                .To<CommandBus>()
                .InRequestOrJobScope();

            Bind<IApplicationStartListener>()
                .To<CommandHandlerDiscovery>()
                .InSingletonScope();

            Bind<IEventBus>()
                .To<EventBus>()
                .InRequestOrJobScope();

            Bind<IUnitOfWorkFactory>()
                .To<UnitOfWorkFactory>()
                .InRequestOrJobScope();

            Bind<IUnitOfWork>()
                .ToMethod(ctx => ctx.Kernel.Get<IUnitOfWork>())
                .InRequestOrJobScope();

            Bind<IPublishEventBuffer>()
                .ToMethod(ctx => ctx.Kernel.Get<IUnitOfWork>().EventBuffer)
                .InRequestOrJobScope();
        }
    }
}
