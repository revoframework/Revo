using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.Jobs;

namespace Revo.Hangfire
{
    [AutoLoadModule(false)]
    public class HangfireModule : NinjectModule
    {
        private readonly HangfireConfigurationSection configurationSection;

        public HangfireModule(HangfireConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

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
