using GTRevo.Core.Core;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
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
        }
    }
}
