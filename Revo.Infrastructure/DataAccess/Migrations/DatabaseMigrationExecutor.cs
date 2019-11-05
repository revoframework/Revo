using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationExecutor : IDatabaseMigrationExecutor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseMigrationProvider[] migrationProviders;
        private readonly IDatabaseMigrationRegistry migrationRegistry;
        private readonly IDatabaseMigrationDiscovery[] migrationDiscoveries;
        private readonly IDatabaseMigrationSelector migrationSelector;
        private readonly DatabaseMigrationsConfiguration configuration;

        public DatabaseMigrationExecutor(IDatabaseMigrationProvider[] migrationProviders,
            IDatabaseMigrationRegistry migrationRegistry,
            IDatabaseMigrationDiscovery[] migrationDiscoveries,
            IDatabaseMigrationSelector migrationSelector,
            DatabaseMigrationsConfiguration configuration)
        {
            this.migrationProviders = migrationProviders;
            this.migrationRegistry = migrationRegistry;
            this.migrationDiscoveries = migrationDiscoveries;
            this.migrationSelector = migrationSelector;
            this.configuration = configuration;
        }

        public async Task ExecuteAsync()
        {
            DiscoverAndRegister();
            await SelectAndExecuteMigrationsAsync();
        }

        private void DiscoverAndRegister()
        {
            int migrationCount = 0;

            foreach (var discovery in migrationDiscoveries)
            {
                var migrations = discovery.DiscoverMigrations();
                foreach (var migration in migrations)
                {
                    migrationRegistry.AddMigration(migration);
                    migrationCount++;
                }
            }

            Logger.Debug($"Discovered {migrationCount} database migrations");
        }

        private async Task SelectAndExecuteMigrationsAsync()
        {
            if (configuration.MigrateOnlySpecifiedModules != null)
            {
                foreach (var migratedModule in configuration.MigrateOnlySpecifiedModules)
                {
                    await MigrateModuleAsync(migratedModule);
                }
            }
            else
            {
                foreach (var moduleName in migrationRegistry.GetAvailableModules())
                {
                    await MigrateModuleAsync(new DatabaseMigrationSpecifier(moduleName, null));
                }
            }
        }

        private async Task MigrateModuleAsync(DatabaseMigrationSpecifier migratedModule)
        {
            foreach (var provider in migrationProviders)
            {
                var environmentTags = configuration.EnvironmentTags
                    .Concat(provider.GetProviderEnvironmentTags())
                    .ToArray();

                var selectedMigrations = await migrationSelector.SelectMigrationsAsync(migratedModule.ModuleName,
                    environmentTags, migratedModule.Version);

                if (selectedMigrations.Count > 0 && selectedMigrations.All(provider.SupportsMigration))
                {
                    var highestVersion = selectedMigrations.Where(x =>
                            string.Equals(x.ModuleName, migratedModule.ModuleName,
                                StringComparison.InvariantCultureIgnoreCase)
                            && x.Version != null)
                        .OrderByDescending(x => x.Version)
                        .FirstOrDefault();

                    Logger.Info($"About to apply {selectedMigrations.Count} migrations to database for module {migratedModule.ModuleName} (up to version {highestVersion?.Version})");
                    await provider.ApplyMigrationsAsync(selectedMigrations);

                    Logger.Info($"Successfully migrated {migratedModule.ModuleName} module database to version {highestVersion?.Version}");
                    return;
                }
            }
        }
    }
}