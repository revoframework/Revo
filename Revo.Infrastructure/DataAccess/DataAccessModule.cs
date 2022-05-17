using Ninject.Modules;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess
{
    public class DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatabaseInitializerLoader, IApplicationStartedListener>()
                .To<DatabaseInitializerLoader>()
                .InSingletonScope();

            Bind<IDatabaseInitializerDiscovery>()
                .To<DatabaseInitializerDiscovery>()
                .InSingletonScope();

            Bind<IDatabaseInitializerSorter>()
                .To<DatabaseInitializerDependencySorter>()
                .InSingletonScope();
        }
    }
}
