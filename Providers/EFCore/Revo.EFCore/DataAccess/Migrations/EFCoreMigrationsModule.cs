using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Configuration;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.EFCore.DataAccess.Migrations
{
    [AutoLoadModule(false)]
    public class EFCoreMigrationsModule(EFCoreDataAccessConfigurationSection section) : NinjectModule
    {
        public override void Load()
        {
            Bind<IDatabaseMigrationProvider>()
                .To<EFCoreDatabaseMigrationProvider>()
                .InTaskScope();

            Bind<IMigrationScripterFactory>()
                .To<MigrationScripterFactory>()
                .InSingletonScope();
        }
    }
}