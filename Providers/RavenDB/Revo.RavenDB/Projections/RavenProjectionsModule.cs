using Ninject.Modules;
using Revo.Core.Core;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.RavenDB.Projections
{
    [AutoLoadModule(false)]
    public class RavenProjectionsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, RavenProjectionEventListener.RavenProjectionEventSequencer>()
                .To<RavenProjectionEventListener.RavenProjectionEventSequencer>()
                .InRequestOrJobScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<RavenProjectionEventListener>()
                .InRequestOrJobScope();
        }
    }
}
