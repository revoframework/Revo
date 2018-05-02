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
using Revo.Integrations.Rebus.Events;

namespace Revo.Integrations.Rebus
{
    public class RebusModule : NinjectModule
    {
        public override void Load()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"]?.ConnectionString;
            if (connectionString?.Length > 0)
            {
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

                Dictionary<string, string> connectionParams = connectionString.Split(';')
                    .Select(value => value.Split('='))
                    .ToDictionary(pair => pair[0].Trim(), pair => pair.Length > 0 ? pair[1].Trim() : null);

                var bus = Configure.With(Kernel.Get<IContainerAdapter>())
                    .Logging(l => l.NLog())
                    .Transport(t =>
                        t.UseRabbitMq(
                            connectionParams.TryGetValue("Url", out string url) ? url : "amqp://localhost",
                            connectionParams.TryGetValue("InputQueue", out string database) ? database : "Revo"))
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
}
