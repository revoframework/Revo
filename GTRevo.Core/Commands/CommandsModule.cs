using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core.Lifecycle;
using Ninject.Modules;

namespace GTRevo.Core.Commands
{
    public class CommandsModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IApplicationStartListener>()
                .To<CommandHandlerDiscovery>()
                .InSingletonScope();
        }
    }
}
