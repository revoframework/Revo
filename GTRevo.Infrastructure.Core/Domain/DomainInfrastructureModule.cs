using GTRevo.Core.Core.Lifecycle;
using GTRevo.Infrastructure.Core.Domain.Events;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Core.Domain
{
    public class DomainInfrastructureModule : NinjectModule
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
