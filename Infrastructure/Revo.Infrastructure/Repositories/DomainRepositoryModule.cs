using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Transactions;

namespace Revo.Infrastructure.Repositories
{
    public class DomainRepositoryModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IRepository, IUnitOfWorkProvider>()
                .To<Repository>()
                .InRequestOrJobScope();

            Bind<IRepositoryFactory>()
                .To<RepositoryFactory>()
                .InTransientScope();

            Bind<IAggregateStore>()
                .To<EventSourcedAggregateStore>()
                .InTransientScope();

            Bind<IEntityFactory>()
                .To<EntityFactory>()
                .InSingletonScope();
        }
    }
}
