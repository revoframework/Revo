using System;
using Newtonsoft.Json;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.EFCore.Events
{
    [AutoLoadModule(false)]
    public class EFCoreAsyncEventsModule(Func<JsonSerializerSettings, JsonSerializerSettings> customizeEventJsonSerializer)
        : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<EFCoreAsyncEventQueueManager>()
                .InTaskScope();

            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope()
                .WithConstructorArgument(customizeEventJsonSerializer);

            Bind<IQueuedAsyncEventMessageFactory>()
                .To<QueuedAsyncEventMessageFactory>()
                .InSingletonScope();

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
