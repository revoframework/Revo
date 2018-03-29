using Ninject;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.Sagas
{
    public class SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaLocator>()
                .To<KeySagaLocator>()
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

            Bind<IRepository>()
                .ToMethod(ctx =>
                    ctx.Kernel.Get<IRepositoryFactory>().CreateRepository(ctx.Kernel.Get<IPublishEventBuffer>()))
                .WhenInjectedInto<SagaRepository>()
                .InRequestOrJobScope();

            Bind<ISagaEventDispatcher>()
                .To<SagaEventDispatcher>()
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
