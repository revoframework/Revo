using GTRevo.Platform.Core;
using GTRevo.Transactions;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcingInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventSourcedRepository, ITransactionProvider>()
                .To<EventSourcedRepository>()
                .InRequestOrJobScope();
        }
    }
}
