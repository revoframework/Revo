using GTRevo.Core;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.EventSourcing;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.EventSourcing
{
    public class EF6EventSourcingModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IEventStore>()
                .To<EF6EventStore>()
                .InRequestOrJobScope();

            Bind<ICrudRepository>()
                .To<CrudRepository>()
                .WhenInjectedInto<EF6EventStore>()
                .InTransientScope();
        }
    }
}
