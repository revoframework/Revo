using GTRevo.Core.Core;
using GTRevo.Core.Transactions;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Repositories
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
        }
    }
}
