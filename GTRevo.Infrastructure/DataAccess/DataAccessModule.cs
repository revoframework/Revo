using GTRevo.Core.Core.Lifecycle;
using Ninject.Modules;

namespace GTRevo.Infrastructure.DataAccess
{
    public class DataAccessModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartListener>()
                .To<DatabaseInitializerLoader>()
                .InSingletonScope();

            Bind<IDatabaseInitializerDiscovery>()
                .To<DatabaseInitializerDiscovery>()
                .InSingletonScope();
        }
    }
}
