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
                .InTaskScope();

            Bind<IRepositoryFactory>()
                .To<RepositoryFactory>()
                .InTransientScope();

            Bind<IEntityFactory>()
                .To<EntityFactory>()
                .InSingletonScope();
        }
    }
}
