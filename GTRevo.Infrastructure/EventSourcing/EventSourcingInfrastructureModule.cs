using GTRevo.Core.Transactions;
using GTRevo.Platform.Core;
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
