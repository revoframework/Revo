using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Security.Commands;
using GTRevo.Platform.Commands;
using Ninject.Modules;

namespace GTRevo.Infrastructure.Validation
{
    public class ValidationInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommandFilter<ICommandBase>>()
                .To<CommandAttributeValidationFilter>()
                .InSingletonScope();
        }
    }
}
