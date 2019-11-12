using Ninject.Modules;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Examples.Todos
{
    public class Module : NinjectModule
    {
        public override void Load()
        {
            Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                .ToConstant(new ResourceDatabaseMigrationDiscoveryAssembly(GetType().Assembly, "Sql"))
                .InSingletonScope();
        }
    }
}