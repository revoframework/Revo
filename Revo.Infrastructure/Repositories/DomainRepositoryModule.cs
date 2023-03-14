using Ninject.Modules;
using Revo.Core.Core;

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

            Bind<IEventSourcedAggregateFactory>()
                .To<EventSourcedAggregateFactory>()
                .InSingletonScope();
        }
    }
}
