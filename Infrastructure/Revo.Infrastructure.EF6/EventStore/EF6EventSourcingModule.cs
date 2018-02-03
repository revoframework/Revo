using Ninject.Modules;
using Revo.Core.Core;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.EventStore
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
