using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        private readonly RebusConnectionConfiguration connectionConfiguration;

        public RebusModule(RebusConnectionConfiguration connectionConfiguration)
        {
            this.connectionConfiguration = connectionConfiguration;
        }

        public override void Load()
        {
            var connectionString = connectionConfiguration.ConnectionString
                                   ?? (connectionConfiguration.ConnectionName != null
                                       ? ConfigurationManager.ConnectionStrings[connectionConfiguration.ConnectionName]?.ConnectionString
                                       : null);

            Dictionary<string, string> connectionParams = connectionString?.Split(';')
                .Select(value => value.Split('='))
                .ToDictionary(pair => pair[0].Trim(), pair => pair.Length > 0 ? pair[1].Trim() : null);

            Bind<IAsyncEventListener<IEvent>>()
                .To<RebusEventListener>()
                .InRequestOrJobScope();

            Bind<IAsyncEventSequencer<IEvent>, RebusEventListener.RebusEventSequencer>()
                .To<RebusEventListener.RebusEventSequencer>()
                .InRequestOrJobScope();

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
                .Transport(t =>
                    t.UseRabbitMq(
                        connectionParams != null && connectionParams.TryGetValue("Url", out string url) ? url : "amqp://localhost",
                        connectionParams != null && connectionParams.TryGetValue("InputQueue", out string database) ? database : "Revo"))
                .Routing(r => r.TypeBasedRoutingFromAppConfig())
                .Options(c => c.EnableUnitOfWork(
                    mc =>
                    {
                        var uow = Kernel.Get<RebusUnitOfWork>();
                        mc.TransactionContext.Items["uow"] = uow;
                        return uow;
                    },
                    async (mc, uow) => await uow.RunAsync()))
                .Start();
        }
    }
}
