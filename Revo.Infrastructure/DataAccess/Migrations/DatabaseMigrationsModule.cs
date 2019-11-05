using Ninject.Modules;
using Revo.Core.Lifecycle;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationsModule : NinjectModule
    {
        private readonly DatabaseMigrationsConfiguration configuration;

        public DatabaseMigrationsModule(DatabaseMigrationsConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public override void Load()
        {
            Bind<IDatabaseMigrationsConfiguration>()
                .ToConstant(configuration);

            Bind<IDatabaseMigrationRegistry>()
                .To<DatabaseMigrationRegistry>()
                .InSingletonScope();

            Bind<IDatabaseMigrationSelector>()
                .To<DatabaseMigrationSelector>()
                .InSingletonScope();

            Bind<IDatabaseMigrationExecutor>()
                .To<DatabaseMigrationExecutor>()
                .InSingletonScope();
            
            Bind<IApplicationConfigurer>()
                .To<DatabaseMigrationExecutionHook>()
                .InSingletonScope();

            foreach (var path in configuration.ScannedFilePaths)
            {
                Bind<FileDatabaseMigrationDiscoveryPath>()
                    .ToConstant(path)
                    .InSingletonScope();
            }

            Bind<IDatabaseMigrationDiscovery>()
                .To<FileDatabaseMigrationDiscovery>()
                .InSingletonScope();
            
            foreach (var assembly in configuration.ScannedAssemblies)
            {
                Bind<ResourceDatabaseMigrationDiscoveryAssembly>()
                    .ToConstant(assembly)
                    .InSingletonScope();
            }

            Bind<IDatabaseMigrationDiscovery>()
                .To<ResourceDatabaseMigrationDiscovery>()
                .InSingletonScope();
        }
    }
}