using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Lifecycle;
using Revo.Core.Transactions;
using Revo.Infrastructure.Jobs;

namespace Revo.Infrastructure.Events.Async
{
    public class AsyncEventsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartedListener>()
                .To<AsyncEventExecutionCatchUp>()
                .InSingletonScope();

            Bind<IAsyncEventWorkerLockCache>()
                .To<AsyncEventWorkerLockCache>()
                .InSingletonScope();

            Bind<IAsyncEventWorker>()
                .To<LockingAsyncEventWorker>()
                .InTaskScope();

            Bind<IAsyncEventWorker>()
                .To<AsyncEventWorker>()
                .WhenInjectedExactlyInto<LockingAsyncEventWorker>()
                .InTransientScope();

            Bind<IAsyncEventQueueDispatcher>()
                .To<AsyncEventQueueDispatcher>()
                .InTaskScope();

            Bind<IEventListener<IEvent>, IUnitOfWorkListener>()
                .To<AsyncEventQueueDispatchListener>()
                .InTaskScope();
            
            Bind<IAsyncEventProcessor>()
                .To<AsyncEventProcessor>()
                .InTaskScope();

            Bind<IJobHandler<ProcessAsyncEventsJob>>()
                .To<ProcessAsyncEventsJobHandler>()
                .InTransientScope();
        }
    }
}
