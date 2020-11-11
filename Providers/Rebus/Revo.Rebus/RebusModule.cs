using System;
using Ninject;
using Ninject.Modules;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Ninject;
using Rebus.NLog.Config;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Rebus.Events;

namespace Revo.Rebus
{
    public class RebusModule : NinjectModule
    {
        private readonly Func<RebusConfigurer, RebusConfigurer> configureFunc;

        public RebusModule(Func<RebusConfigurer, RebusConfigurer> configureFunc)
        {
            this.configureFunc = configureFunc;
        }

        public override void Load()
        {
            Bind<IAsyncEventListener<IEvent>>()
                .To<RebusEventListener>()
                .InTaskScope();

            Bind<IAsyncEventSequencer<IEvent>, RebusEventListener.RebusEventSequencer>()
                .To<RebusEventListener.RebusEventSequencer>()
                .InTaskScope();

            Bind<IHandleMessages<IEvent>>()
                .To<RebusEventMessageHandler>()
                .InTransientScope();

            Bind<IContainerAdapter>()
                .To<NinjectContainerAdapter>()
                .InSingletonScope();

            Bind<RebusUnitOfWork>()
                .ToSelf()
                .InTransientScope();

            var bus = Configure.With(Kernel.Get<IContainerAdapter>())
                .Logging(l => l.NLog())
                .Options(c => c.EnableUnitOfWork(
                    mc =>
                    {
                        var uow = Kernel.Get<RebusUnitOfWork>();
                        mc.TransactionContext.Items["uow"] = uow;
                        return uow;
                    },
                    async (mc, uow) => await uow.RunAsync()));

            if (configureFunc != null)
            {
                bus = configureFunc(bus);
            }

            bus.Start();
        }
    }
}
