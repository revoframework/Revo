using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EF6.EventStores
{
    [AutoLoadModule(false)]
    public class EF6EventStoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventStore>()
                .To<EF6EventStore>()
                .InTaskScope();

            Bind<IExternalEventStore>()
                .To<ExternalEventStore>()
                .InTaskScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EF6EventStore>()
                .InTransientScope();

            Bind<IEventSourceCatchUp>()
                .To<EventSourceCatchUp>()
                .InTransientScope();

            Bind<IEventSourceCatchUp>()
                .To<ExternalEventSourceCatchUp>()
                .InTransientScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EventSourceCatchUp>()
                .InTransientScope();

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
