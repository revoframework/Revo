using Ninject.Modules;
using Revo.Core.Core;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure;
using Revo.Infrastructure.DataAccess.Migrations;
using Revo.Infrastructure.Sagas;
using Revo.Infrastructure.Sagas.Generic;

namespace Revo.EF6.Sagas
{
    [AutoLoadModule(false)]
    public class EF6SagasModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISagaMetadataRepository>()
                .To<SagaMetadataRepository>()
                .InTaskScope();

            Bind<IEF6CrudRepository>()
                .To<EF6CrudRepository>()
                .WhenInjectedInto<SagaMetadataRepository>()
                .InTransientScope();

            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(
                    typeof(InfrastructureConfigurationSection).Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}
