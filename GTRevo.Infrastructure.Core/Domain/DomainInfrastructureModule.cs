using GTRevo.Core.Lifecycle;
using GTRevo.Infrastructure.Domain.Events;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Domain
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
