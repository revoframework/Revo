using System;
using System.Collections.Generic;
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
        private readonly IDatabaseMigrationExecutionOptions options;
        private bool hasDiscoveryRun = false;

        public DatabaseMigrationExecutor(IDatabaseMigrationProvider[] migrationProviders,
            IDatabaseMigrationRegistry migrationRegistry,
            IDatabaseMigrationDiscovery[] migrationDiscoveries,
            IDatabaseMigrationSelector migrationSelector,
            IDatabaseMigrationExecutionOptions options)
        {
            this.migrationProviders = migrationProviders;
            this.migrationRegistry = migrationRegistry;
            this.migrationDiscoveries = migrationDiscoveries;
            this.migrationSelector = migrationSelector;
            this.options = options;
        }

        public async Task<IReadOnlyCollection<PendingModuleMigration>> ExecuteAsync()
        {
            DiscoverAndRegister();
            return await SelectAndExecuteMigrationsAsync();
        }

        public async Task<IReadOnlyCollection<PendingModuleMigration>> PreviewAsync()
        {
            DiscoverAndRegister();

            var migrations = await SelectMigrationsAsync();
            return migrations.ToArray();
        }

        private void DiscoverAndRegister()
        {
            if (hasDiscoveryRun)
            {
                return;
            }

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
            hasDiscoveryRun = true;
        }

        private async Task<IReadOnlyCollection<PendingModuleMigration>> SelectAndExecuteMigrationsAsync()
        {
            var migrations = await SelectMigrationsAsync();

            foreach (var migration in migrations)
            {
                await MigrateModuleAsync(migration.Specifier, migration.Migrations, migration.Provider);
            }

            return migrations;
        }
        
        private async Task<IReadOnlyCollection<PendingModuleMigration>> SelectMigrationsAsync()
        {
            var migrations = new List<PendingModuleMigration>();

            if (options.MigrateOnlySpecifiedModules != null)
            {
                foreach (var migratedModule in options.MigrateOnlySpecifiedModules)
                {
                    var moduleMigrations = await SelectModuleMigrationsAsync(migratedModule);

                    if (moduleMigrations.Migrations.Count > 0)
                    {
                        migrations.Add(new PendingModuleMigration()
                        {
                            Migrations = moduleMigrations.Migrations,
                            Provider = moduleMigrations.Provider,
                            Specifier = migratedModule
                        });
                    }
                }
            }
            else
            {
                foreach (var moduleName in migrationRegistry.GetAvailableModules())
                {
                    var migratedModule = new DatabaseMigrationSpecifier(moduleName, null);
                    var moduleMigrations = await SelectModuleMigrationsAsync(migratedModule);

                    if (moduleMigrations.Migrations.Count > 0)
                    {
                        migrations.Add(new PendingModuleMigration()
                        {
                            Migrations = moduleMigrations.Migrations,
                            Provider = moduleMigrations.Provider,
                            Specifier = migratedModule
                        });
                    }
                }
            }

            return migrations;
        }

        private async Task<(IReadOnlyCollection<IDatabaseMigration> Migrations, IDatabaseMigrationProvider Provider)> SelectModuleMigrationsAsync(DatabaseMigrationSpecifier migratedModule)
        {
            foreach (var provider in migrationProviders)
            {
                var environmentTags = options.EnvironmentTags
                    .Concat(provider.GetProviderEnvironmentTags())
                    .ToArray();

                var selectedMigrations = await migrationSelector.SelectMigrationsAsync(migratedModule.ModuleName,
                    environmentTags, migratedModule.Version);

                if (selectedMigrations.Count > 0 && selectedMigrations.All(provider.SupportsMigration))
                {
                    return (selectedMigrations, provider);
                }
            }

            return (new IDatabaseMigration[0], null);
        }

        private async Task MigrateModuleAsync(DatabaseMigrationSpecifier migratedModule, IReadOnlyCollection<IDatabaseMigration> selectedMigrations, IDatabaseMigrationProvider provider)
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
        }
    }
}