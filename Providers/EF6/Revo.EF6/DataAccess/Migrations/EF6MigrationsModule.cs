using Ninject.Modules;
using Revo.Core.Core;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.EF6.DataAccess.Migrations
{
    [AutoLoadModule(false)]
    public class EF6MigrationsModule(EF6DataAccessConfigurationSection section) : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatabaseMigrationProvider>()
                .To<EF6DatabaseMigrationProvider>()
                .InTaskScope();

            Bind<IMigrationScripterFactory>()
                .To<MigrationScripterFactory>()
                .InSingletonScope();
        }
    }
}