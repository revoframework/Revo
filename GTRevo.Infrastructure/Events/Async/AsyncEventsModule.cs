using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Jobs;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Events.Async
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
