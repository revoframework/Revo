using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.Configuration;
using Revo.EFCore.Repositories;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore
{
    [AutoLoadModule(false)]
    public class EFCoreRepositoriesModule : NinjectModule
    {
        private readonly EFCoreInfrastructureConfigurationSection configurationSection;

        public EFCoreRepositoriesModule(EFCoreInfrastructureConfigurationSection configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            if (configurationSection.UseCrudAggregateStore)
            {
                Bind<IAggregateStoreFactory>()
                    .To<EFCoreCrudAggregateStoreFactory>()
                    .InTransientScope();
            }

            if (configurationSection.UseEventSourcedAggregateStore)
            {
                Bind<IAggregateStoreFactory>()
                    .To<EFCoreEventSourcedAggregateStoreFactory>()
                    .InTransientScope();
            }
        }
    }
}
