using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Sagas
{
    public class SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaLocator>()
                .To<SagaLocator>()
                .InRequestOrJobScope();

            Bind<ISagaRegistry>()
                .To<SagaRegistry>()
                .InSingletonScope();

            Bind<ISagaConfigurator>()
                .To<SagaConventionConfigurator>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<SagaConfigurationLoader>()
                .InSingletonScope();

            Bind<ISagaRepository>() //itransactionprovider?
                .To<SagaRepository>()
                .InRequestOrJobScope();

            Bind<IAsyncEventSequencer<DomainEvent>, SagaEventListener.SagaEventSequencer>()
                .To<SagaEventListener.SagaEventSequencer>()
                .InRequestOrJobScope();

            Bind<IAsyncEventListener<DomainEvent>>()
                .To<SagaEventListener>()
                .InRequestOrJobScope();
        }
    }
}
