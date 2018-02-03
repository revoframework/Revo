using Ninject.Modules;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Repositories;

namespace Revo.Infrastructure.EF6.Repositories
{
    public class DomainRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<EF6AggregateStore>()
                .InTransientScope();

            Bind<IAggregateStore>()
                .To<EF6AggregateStore>()
                .InTransientScope();
        }
    }
}
