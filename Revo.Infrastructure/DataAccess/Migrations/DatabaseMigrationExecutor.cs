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
            var pendingModuleMigrations = await SelectMigrationsAsync();

            foreach (var pendingModuleMigration in pendingModuleMigrations)
            {
                await MigrateModuleAsync(pendingModuleMigration);
            }

            return pendingModuleMigrations;
        }
        
        private async Task<IReadOnlyCollection<PendingModuleMigration>> SelectMigrationsAsync()
        {
            IReadOnlyCollection<PendingModuleMigration> migrations;
            if (options.MigrateOnlySpecifiedModules != null)
            {
                migrations = await SelectModuleMigrationsAsync(options.MigrateOnlySpecifiedModules.ToArray());
            }
            else
            {
                migrations = await SelectModuleMigrationsAsync(
                    migrationRegistry.GetAvailableModules()
                        .Select(x => new DatabaseMigrationSpecifier(x, null)).ToArray());
            }

            return migrations;
        }

        private async Task<IReadOnlyCollection<PendingModuleMigration>> SelectModuleMigrationsAsync(DatabaseMigrationSpecifier[] migratedModules)
        {
            var migrations = new List<PendingModuleMigration>();
            foreach (var provider in migrationProviders)
            {
                var environmentTags = options.EnvironmentTags
                    .Concat(provider.GetProviderEnvironmentTags())
                    .ToArray();

                var selectedMigrations = await migrationSelector.SelectMigrationsAsync(migratedModules, environmentTags);

                if (selectedMigrations.Count > 0 && selectedMigrations.SelectMany(x => x.Migrations)
                        .All(provider.SupportsMigration))
                {
                    migrations.AddRange(selectedMigrations.Select(x => new PendingModuleMigration(x.Specifier, x.Migrations, provider)));
                }
            }

            return migrations;
        }

        private async Task MigrateModuleAsync(PendingModuleMigration migration)
        {
            var highestVersion = migration.Migrations.Where(x =>
                    string.Equals(x.ModuleName, migration.Specifier.ModuleName,
                        StringComparison.InvariantCultureIgnoreCase)
                    && x.Version != null)
                .OrderByDescending(x => x.Version)
                .FirstOrDefault();

            Logger.Info($"About to apply {migration.Migrations.Count} migrations to database for module {migration.Specifier.ModuleName} (up to version {highestVersion?.Version})");

            await migration.Provider.ApplyMigrationsAsync(migration.Migrations);

            Logger.Info($"Successfully migrated {migration.Specifier.ModuleName} module database to version {highestVersion?.Version}");
        }
    }
}