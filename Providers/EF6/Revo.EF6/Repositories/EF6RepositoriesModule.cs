using Ninject.Modules;
using Revo.Core.Core;
using Revo.EF6.Configuration;
using Revo.Infrastructure.Repositories;

namespace Revo.EF6.Repositories
{
    [AutoLoadModule(false)]
    public class EF6RepositoriesModule : NinjectModule
    {
        private readonly EF6InfrastructureConfigurationSection configurationSection;

        public EF6RepositoriesModule(EF6InfrastructureConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            // TODO ensure the aggregate stores get injected the correct EF6 repository/event store (in case there are more registered)

            if (configurationSection.UseCrudAggregateStore)
            {
                Bind<IAggregateStoreFactory>()
                    .To<CrudAggregateStoreFactory>()
                    .InTransientScope();
            }

            if (configurationSection.UseEventSourcedAggregateStore)
            {
                Bind<IAggregateStoreFactory>()
                    .To<EventSourcedAggregateStoreFactory>()
                    .InTransientScope();
            }
        }
    }
}
