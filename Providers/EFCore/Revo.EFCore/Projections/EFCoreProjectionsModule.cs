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
            Bind<IAsyncEventSequencer<DomainAggregateEvent>, EFCoreProjectionEventListener.EFCoreProjectionEventSequencer>()
                .To<EFCoreProjectionEventListener.EFCoreProjectionEventSequencer>()
                .InTaskScope();

            Bind<IAsyncEventListener<DomainAggregateEvent>>()
                .To<EFCoreProjectionEventListener>()
                .InTaskScope();

            Bind<IEFCoreProjectionSubSystem>()
                .To<EFCoreProjectionSubSystem>()
                .InTaskScope();
        }
    }
}
