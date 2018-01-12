using GTRevo.Core.Core;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.Events.Metadata;
using GTRevo.Platform.Core;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcingInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventSourcedAggregateRepository, ITransactionProvider>()
                .To<EventSourcedAggregateRepository>()
                .InRequestOrJobScope();

            Bind<IEventMessageFactory>()
                .To<EventMessageFactory>()
                .InRequestOrJobScope();

            Bind<IEventMetadataProvider>()
                .To<ActorNameEventMetadataProvider>()
                .InTransientScope();

            Bind<IEventMetadataProvider>()
                .To<UserIdEventMetadataProvider>()
                .InTransientScope();
        }
    }
}
