using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Platform.Core;
using MediatR;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Projections
{
    public class DomainInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            // TODO eliminate the need to double register
            Bind<IEventListener<DomainAggregateEvent>, IAsyncNotificationHandler<DomainAggregateEvent>,
                IEventQueueTransactionListener>()
                .To<ProjectionEventListener>()
                .InRequestOrJobScope();
        }
    }
}
