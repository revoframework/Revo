using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    [AutoLoadModule(false)]
    public class HangfireModule(HangfireConfigurationSection configurationSection) : NinjectModule
    {
        public override void Load()
        {
            Bind<HangfireConfigurationSection>()
                .ToConstant(configurationSection)
                .InSingletonScope();

            Bind<IJobScheduler>()
                .To<HangfireJobScheduler>()
                .InTaskScope();
        }
    }
}
