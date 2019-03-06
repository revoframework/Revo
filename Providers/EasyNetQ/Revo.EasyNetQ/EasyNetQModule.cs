using EasyNetQ;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.EasyNetQ.Configuration;

namespace Revo.EasyNetQ
{
    [AutoLoadModule(false)]
    public class EasyNetQModule : NinjectModule
    {
        private readonly EasyNetQConfigurationSection configurationSection;

        public EasyNetQModule(EasyNetQConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            Bind<EasyNetQConfigurationSection>()
                .ToConstant(configurationSection);

            if (configurationSection.IsActive)
            {
                Bind<IApplicationStartListener, IApplicationStopListener>()
                    .To<BusInitializer>()
                    .InSingletonScope();

                Bind<IBus>()
                    .ToMethod(ctx => RabbitHutch.CreateBus(configurationSection.Connection.ConnectionString))
                    .InSingletonScope();

                Bind<IEasyNetQBus>()
                    .To<EasyNetQBus>()
                    .InTaskScope();

                Bind<IEasyNetQSubscriptionHandler>()
                    .To<EasyNetQSubscriptionHandler>()
                    .InSingletonScope();

                foreach (var eventTransportPair in configurationSection.EventTransports.Events)
                {
                    Bind(typeof(IEventListener<>).MakeGenericType(eventTransportPair.Key))
                        .To(typeof(EventTransport<,>).MakeGenericType(eventTransportPair.Key, eventTransportPair.Value))
                        .InTaskScope();
                }
            }
            else
            {
                Bind<IEasyNetQBus>()
                    .To<NullEasyNetQBus>()
                    .InSingletonScope();
            }
        }
    }
}
