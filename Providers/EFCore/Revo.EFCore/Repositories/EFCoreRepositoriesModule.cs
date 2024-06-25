using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.Configuration;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    [AutoLoadModule(false)]
    public class EFCoreRepositoriesModule(EFCoreInfrastructureConfigurationSection configurationSection) : NinjectModule
    {
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
