using System;
using Ninject;
using Ninject.Extensions.ContextPreservation;
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
                .ToMethod(ctx => ctx.ContextPreservingGet<ICommandContext>().UnitOfWork ?? throw new InvalidOperationException("Trying to resolve IUnitOfWork when there is no instance active in current command context"))
                .InTransientScope();

            Bind<IPublishEventBufferFactory>()
                .To<PublishEventBufferFactory>()
                .InRequestOrJobScope();

            Bind<IPublishEventBuffer>()
                .ToMethod(ctx => ctx.ContextPreservingGet<IUnitOfWork>().EventBuffer)
                .InTransientScope();
        }
    }
}
