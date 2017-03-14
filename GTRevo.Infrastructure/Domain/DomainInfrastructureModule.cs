using GTRevo.Infrastructure.Domain.Projections;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Platform.Core;
using GTRevo.Platform.Core.Lifecycle;
using GTRevo.Platform.Events;
using GTRevo.Platform.Transactions;
using MediatR;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Domain
{
    public class DomainInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventSourcedRepository, ITransactionProvider>()
                .To<EventSourcedRepository>()
                .InRequestOrJobScope();

            Bind<DomainEventTypeCache, IApplicationStartListener>()
                .To<DomainEventTypeCache>()
                .InSingletonScope();

            Bind<IApplicationStartListener>()
                .To<ConventionEventApplyRegistratorCache>()
                .InSingletonScope();

            // TODO eliminate the need to double register
            Bind<IEventListener<DomainAggregateEvent>, IAsyncNotificationHandler<DomainAggregateEvent>,
                IEventQueueTransactionListener>()
                .To<ProjectionEventListener>()
                .InRequestOrJobScope();
        }
    }
}
