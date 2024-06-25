using System;
using Newtonsoft.Json;
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
        private readonly Func<JsonSerializerSettings, JsonSerializerSettings> customizeEventJsonSerializer;

        public EF6AsyncEventsModule(Func<JsonSerializerSettings, JsonSerializerSettings> customizeEventJsonSerializer)
        {
            this.customizeEventJsonSerializer = customizeEventJsonSerializer;
        }

        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<AsyncEventQueueManager>()
                .InTaskScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<AsyncEventQueueManager>()
                .InTransientScope();

            Bind<IQueuedAsyncEventMessageFactory>()
                .To<QueuedAsyncEventMessageFactory>()
                .InSingletonScope();

            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope()
                .WithConstructorArgument(customizeEventJsonSerializer);

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
