using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.EF6.Events
{
    [AutoLoadModule(false)]
    public class EF6AsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<AsyncEventQueueManager>()
                .InTaskScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<AsyncEventQueueManager>()
                .InTransientScope();

            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope();

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly.FullName,
                    "Sql"))
                .InSingletonScope();
        }
    }
}
