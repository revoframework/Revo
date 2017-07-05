using GTRevo.Core.Core.Lifecycle;
using GTRevo.Infrastructure.Core.Domain.Events;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Core.Domain
{
    public class DomainInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DomainEventTypeCache, IApplicationStartListener>()
                .To<DomainEventTypeCache>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<ConventionEventApplyRegistratorCache>()
                .InSingletonScope();
        }
    }
}
