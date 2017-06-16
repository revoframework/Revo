using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Repositories;
using Ninject.Modules;

namespace GTRevo.Infrastructure.EF6.Repositories
{
    public class DomainRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICrudRepository>()
                .To<CrudRepository>()
                .WhenInjectedInto<EF6AggregateStore>()
                .InTransientScope();

            Bind<IAggregateStore>()
                .To<EF6AggregateStore>()
                .InTransientScope();
        }
    }
}
