using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Core.Lifecycle;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartListener>()
                .To<AsyncEventExecutionCatchUp>()
                .InSingletonScope();

            Bind<IAsyncEventQueueBacklogWorker>()
                .To<AsyncEventQueueBacklogWorker>()
                .InRequestOrJobScope();

            Bind<IAsyncEventQueueDispatcher>()
                .To<AsyncEventQueueDispatcher>()
                .InRequestOrJobScope();

            Bind<IEventListener<IEvent>, IUnitOfWorkListener>()
                .To<AsyncEventQueueDispatchListener>()
                .InRequestOrJobScope();
            
            Bind<IAsyncEventProcessor>()
                .To<AsyncEventProcessor>()
                .InRequestOrJobScope();

            Bind<IJobHandler<ProcessAsyncEventsJob>>()
                .To<ProcessAsyncEventsJobHandler>()
                .InTransientScope();
        }
    }
}
