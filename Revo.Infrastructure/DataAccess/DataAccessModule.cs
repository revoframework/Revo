using Ninject.Modules;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess
{
    public class DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatabaseInitializerLoader, IApplicationStartListener>()
                .To<DatabaseInitializerLoader>()
                .InSingletonScope();

            Bind<IDatabaseInitializerDiscovery>()
                .To<DatabaseInitializerDiscovery>()
                .InSingletonScope();

            Bind<IDatabaseInitializerComparer>()
                .To<DatabaseInitializerDependencyComparer>()
                .InSingletonScope();
        }
    }
}
