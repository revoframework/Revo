using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.Sagas
{
    [AutoLoadModule(false)]
    public class SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaLocator>()
                .To<KeySagaLocator>()
                .InTaskScope();

            Bind<ISagaRegistry>()
                .To<SagaRegistry>()
                .InSingletonScope();

            Bind<ISagaConfigurator>()
                .To<ConventionSagaConfigurator>()
                .InSingletonScope();

            Bind<IApplicationStartedListener>()
                .To<SagaConfigurationLoader>()
                .InSingletonScope();

            Bind<ISagaRepository>()
                .To<SagaRepository>()
                .InTransientScope();

            Bind<IRepository>()
                .To<Repository>()
                .WhenInjectedInto<SagaRepository>()
                .InTransientScope();
            
            Bind<ISagaEventDispatcher>()
                .To<SagaEventDispatcher>()
                .InTransientScope();

            Bind<IAsyncEventSequencer<DomainEvent>, SagaEventListener.SagaEventSequencer>()
                .To<SagaEventListener.SagaEventSequencer>()
                .InTaskScope();

            Bind<IAsyncEventListener<DomainEvent>>()
                .To<SagaEventListener>()
                .InTaskScope();
        }
    }
}
