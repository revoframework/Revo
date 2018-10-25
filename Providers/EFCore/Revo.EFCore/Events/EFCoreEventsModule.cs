using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.EFCore.Events
{
    [AutoLoadModule(false)]
    public class EFCoreAsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<AsyncEventQueueManager>()
                .InTaskScope();

            Bind<ICrudRepository>()
                .To<EFCoreCrudRepository>()
                .WhenInjectedInto<AsyncEventQueueManager>()
                .InTransientScope();

            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope();
        }
    }
}
