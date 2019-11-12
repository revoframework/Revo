using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Sagas.Generic;

namespace Revo.EFCore.Sagas
{
    [AutoLoadModule(false)]
    public class EFCoreSagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<SagaMetadataRepository>()
                .InTaskScope();

            Bind<IEFCoreCrudRepository>()
                .To<EFCoreCrudRepository>()
                .WhenInjectedInto<SagaMetadataRepository>()
                .InTransientScope();

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
