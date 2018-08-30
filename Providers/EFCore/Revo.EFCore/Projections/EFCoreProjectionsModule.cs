using Ninject.Modules;
using Revo.Core.Core;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.EFCore.Projections
{
    [AutoLoadModule(false)]
    public class EFCoreProjectionsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, EFCoreProjectionEventListener.EF6ProjectionEventSequencer>()
                .To<EFCoreProjectionEventListener.EF6ProjectionEventSequencer>()
                .InRequestOrJobScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<EFCoreProjectionEventListener>()
                .InRequestOrJobScope();
        }
    }
}
