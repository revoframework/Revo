using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.EF6.Events.Async;
using Revo.Infrastructure.Events.Async;

namespace Revo.EF6.Events
{
    [AutoLoadModule(false)]
    public class EF6AsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventQueueManager>()
                .To<AsyncEventQueueManager>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<AsyncEventQueueManager>()
                .InTransientScope();

            Bind<IEventSerializer>()
                .To<EventSerializer>()
                .InSingletonScope();
        }
    }
}
