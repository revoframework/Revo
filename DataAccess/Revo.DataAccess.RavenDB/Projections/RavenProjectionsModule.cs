using Revo.Core.Core;
using Revo.Infrastructure.Events.Async;
using Ninject.Modules;
using Revo.Domain.Events;

namespace Revo.DataAccess.RavenDB.Projections
{
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
