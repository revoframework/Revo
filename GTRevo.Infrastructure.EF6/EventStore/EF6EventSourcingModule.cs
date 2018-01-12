using GTRevo.Core.Core;
using GTRevo.Core.Core.Lifecycle;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventStore;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.EventStore
{
    public class EF6EventSourcingModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventStore>()
                .To<EF6EventStore>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EF6EventStore>()
                .InTransientScope();

            Bind<IEventSourceCatchUp>()
                .To<EF6EventSourceCatchUp>()
                .InTransientScope();

            Bind<IEF6CrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EF6EventSourceCatchUp>()
                .InTransientScope();

        }
    }
}
