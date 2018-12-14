using Ninject.Modules;
using Revo.Core.Core;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.EF6.Projections
{
    [AutoLoadModule(false)]
    public class EF6ProjectionsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, EF6ProjectionEventListener.EF6ProjectionEventSequencer>()
                .To<EF6ProjectionEventListener.EF6ProjectionEventSequencer>()
                .InTaskScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<EF6ProjectionEventListener>()
                .InTaskScope();
            
            Bind<IEF6ProjectionSubSystem>()
                .To<EF6ProjectionSubSystem>()
                .InTaskScope();
        }
    }
}
