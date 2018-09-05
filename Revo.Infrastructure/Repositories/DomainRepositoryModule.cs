using Ninject.Extensions.ContextPreservation;
using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Repositories
{
    public class DomainRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository>()
                .To<Repository>()
                .InRequestOrJobScope();

            Bind<IRepositoryFactory>()
                .To<RepositoryFactory>()
                .InTransientScope();

            Bind<IAggregateStoreFactory>()
                .To<CrudAggregateStoreFactory>()
                .InTransientScope();
            
            Bind<IAggregateStoreFactory>()
                .To<EventSourcedAggregateStoreFactory>()
                .InTransientScope();

            Bind<IEntityFactory>()
                .To<EntityFactory>()
                .InSingletonScope();
        }
    }
}
