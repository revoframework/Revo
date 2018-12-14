using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EFCore.EventStores
{
    [AutoLoadModule(false)]
    public class EFCoreEventStoreModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventStore>()
                .To<EFCoreEventStore>()
                .InTaskScope();

            Bind<IExternalEventStore>()
                .To<EFCoreExternalEventStore>()
                .InTaskScope();

            Bind<IEventSourceCatchUp>()
                .To<EventSourceCatchUp>()
                .InTransientScope();

            Bind<IEventSourceCatchUp>()
                .To<ExternalEventSourceCatchUp>()
                .InTransientScope();

            Bind<IReadRepository>()
                .To<EFCoreCrudRepository>()
                .WhenInjectedInto<EventSourceCatchUp>()
                .InTransientScope();
        }
    }
}
