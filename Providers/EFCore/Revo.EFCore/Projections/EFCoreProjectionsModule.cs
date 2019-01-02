using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Domain.Events;
using Revo.EFCore.Configuration;
using Revo.Infrastructure.Events.Async;

namespace Revo.EFCore.Projections
{
    [AutoLoadModule(false)]
    public class EFCoreProjectionsModule : NinjectModule
    {
        private readonly EFCoreInfrastructureConfigurationSection configurationSection;

        public EFCoreProjectionsModule(EFCoreInfrastructureConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, EFCoreProjectionEventListener.EFCoreProjectionEventSequencer>()
                .To<EFCoreProjectionEventListener.EFCoreProjectionEventSequencer>()
                .InTaskScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<EFCoreProjectionEventListener>()
                .InTaskScope();

            Bind<IEFCoreProjectionSubSystem>()
                .To<EFCoreProjectionSubSystem>()
                .InTaskScope();

            Bind<IEFCoreProjectorResolver>()
                .To<EFCoreProjectorResolver>()
                .InTransientScope();

            if (configurationSection.AutoDiscoverProjectors)
            {
                Bind<IApplicationConfigurer>()
                    .To<EFCoreProjectorDiscovery>()
                    .InSingletonScope();
            }
        }
    }
}
