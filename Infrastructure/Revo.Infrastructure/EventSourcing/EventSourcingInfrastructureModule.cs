using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.EventSourcing
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
