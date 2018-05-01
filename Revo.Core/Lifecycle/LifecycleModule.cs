using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;

namespace Revo.Core.Lifecycle
{
    public class LifecycleModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationConfigurerInitializer>()
                .To<ApplicationConfigurerInitializer>()
                .InSingletonScope();

            Bind<IApplicationStartListenerInitializer>()
                .To<ApplicationStartListenerInitializer>()
                .InSingletonScope();
        }
    }
}
