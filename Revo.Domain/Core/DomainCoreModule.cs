using Ninject.Modules;
using Revo.Core.Lifecycle;
using Revo.Domain.Events;
using Revo.Domain.Sagas;

namespace Revo.Domain.Core
{
    public class DomainCoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDomainEventTypeCache, IApplicationStartListener>()
                .To<DomainEventTypeCache>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<ConventionEventApplyRegistratorCache>()
                .InSingletonScope();

            Bind<ISagaConventionConfigurationCache, IApplicationStartListener>()
                .To<SagaConventionConfigurationCache>()
                .InSingletonScope();
        }
    }
}
