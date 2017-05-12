using GTRevo.DataAccess.EF6;
using GTRevo.Platform.Core;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EventSourcing.EF6
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
