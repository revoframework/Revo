using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Lifecycle;
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
