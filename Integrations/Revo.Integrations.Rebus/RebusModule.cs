using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
using Ninject.Modules;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Ninject;
using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Integrations.Rebus.Events;

namespace Revo.Integrations.Rebus
{
    public class RebusModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IHandleMessages<IEvent>>()
                .To<RebusEventMessageHandler>()
                .InTransientScope();

            Bind<IContainerAdapter>()
                .To<NinjectContainerAdapter>()
                .InSingletonScope();

            Bind<RebusUnitOfWork>()
                .ToSelf()
                .InTransientScope();
            
            Bind<IBus>()
                .ToMethod(context =>
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["RabbitMQ"].ConnectionString;
                    Dictionary<string, string> connectionParams = connectionString.Split(';')
                        .Select(value => value.Split('='))
                        .ToDictionary(pair => pair[0].Trim(), pair => pair.Length > 0 ? pair[1].Trim() : null);

                    IBus bus = Configure.With(context.Kernel.Get<IContainerAdapter>())
                        .Transport(t =>
                            t.UseRabbitMq(
                                connectionParams.TryGetValue("Url", out string url) ? url : "amqp://localhost",
                                connectionParams.TryGetValue("InputQueue", out string database) ? database : "Revo"))
                        .Routing(r => r.TypeBasedRoutingFromAppConfig())
                        .Options(c => c.EnableUnitOfWork(
                            mc =>
                            {
                                var uow = context.Kernel.Get<RebusUnitOfWork>();
                                mc.TransactionContext.Items["uow"] = uow;
                                return uow;
                            },
                            async (mc, uow) => await uow.RunAsync()))
                        .Start();
                    return bus;
                })
                .InSingletonScope();
        }
    }
}
