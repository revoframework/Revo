using Ninject;
using Ninject.Modules;
using Revo.Core.Commands;
using Revo.Core.Core.Lifecycle;
using Revo.Core.Events;
using Revo.Core.Transactions;

namespace Revo.Core.Core
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IClock>()
                .ToMethod(ctx => Clock.Current)
                .InTransientScope();

            Bind<IApplicationStartListener>()
                .To<CommandHandlerDiscovery>()
                .InSingletonScope();

            Bind<IEventBus>()
                .To<EventBus>()
                .InRequestOrJobScope();

            Bind<ICommandContext, CommandContextStack>()
                .To<CommandContextStack>()
                .InRequestOrJobScope();

            Bind<IUnitOfWorkFactory>()
                .To<UnitOfWorkFactory>()
                .InRequestOrJobScope();

            Bind<IUnitOfWork>()
                .ToMethod(ctx => ctx.Kernel.Get<ICommandContext>().UnitOfWork)
                .InTransientScope();

            Bind<IPublishEventBuffer>()
                .To<PublishEventBuffer>()
                .InRequestOrJobScope();
        }
    }
}
