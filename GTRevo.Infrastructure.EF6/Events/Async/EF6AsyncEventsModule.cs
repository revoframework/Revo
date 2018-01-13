using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Events.Async;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.Events.Async
{
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
        }
    }
}
