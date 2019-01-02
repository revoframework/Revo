using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.RavenDB.Configuration;

namespace Revo.RavenDB.Projections
{
    [AutoLoadModule(false)]
    public class RavenProjectionsModule : NinjectModule
    {
        private readonly RavenConfigurationSection configurationSection;

        public RavenProjectionsModule(RavenConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, RavenProjectionEventListener.RavenProjectionEventSequencer>()
                .To<RavenProjectionEventListener.RavenProjectionEventSequencer>()
                .InTaskScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<RavenProjectionEventListener>()
                .InTaskScope();

            Bind<IRavenProjectionSubSystem>()
                .To<RavenProjectionSubSystem>()
                .InTaskScope();

            if (configurationSection.AutoDiscoverProjectors)
            {
                Bind<IApplicationConfigurer>()
                    .To<RavenProjectorDiscovery>()
                    .InSingletonScope();
            }
        }
    }
}
