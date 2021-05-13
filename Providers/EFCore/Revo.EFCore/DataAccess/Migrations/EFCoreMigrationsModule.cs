using Ninject.Modules;
using Revo.Core.Core;
using Revo.EFCore.DataAccess.Configuration;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.EFCore.DataAccess.Migrations
{
    [AutoLoadModule(false)]
    public class EFCoreMigrationsModule : NinjectModule
    {
        private readonly EFCoreDataAccessConfigurationSection section;

        public EFCoreMigrationsModule(EFCoreDataAccessConfigurationSection section)
        {
            this.section = section;
        }

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