using Ninject.Modules;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Validation
{
    public class ValidationInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IPreCommandFilter<ICommandBase>>()
                .To<CommandAttributeValidationFilter>()
                .InSingletonScope();
        }
    }
}
