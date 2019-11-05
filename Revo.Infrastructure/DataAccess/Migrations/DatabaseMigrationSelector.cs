using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationSelector : IDatabaseMigrationSelector
    {
        private readonly IDatabaseMigrationRegistry migrationRegistry;
        private readonly IDatabaseMigrationProvider migrationProvider;
        private IReadOnlyCollection<IDatabaseMigrationRecord> history = null;

        public DatabaseMigrationSelector(IDatabaseMigrationRegistry migrationRegistry, IDatabaseMigrationProvider migrationProvider)
        {
            this.migrationRegistry = migrationRegistry;
            this.migrationProvider = migrationProvider;
        }
        
        public async Task<IReadOnlyCollection<IDatabaseMigration>> SelectMigrationsAsync(string moduleName, string[] tags,
            DatabaseVersion targetVersion = null)
        {
            try
            {
                return await DoSelectMigrationsAsync(moduleName, tags, targetVersion, new List<DatabaseMigrationSpecifier>());
            }
            finally
            {
                history = null;
            }
        }

        private async Task<IReadOnlyCollection<IDatabaseMigration>> DoSelectMigrationsAsync(string moduleName, string[] tags,
            DatabaseVersion targetVersion, List<DatabaseMigrationSpecifier> queuedMigrations)
        {
            IEnumerable<IDatabaseMigration> moduleMigrations = migrationRegistry.Migrations
                .Where(x => string.Equals(x.ModuleName, moduleName, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Tags.All(tagGroup => tags.Any(tagGroup.Contains)));

            if (targetVersion != null)
            {
                moduleMigrations = moduleMigrations.Where(x => x.Version.CompareTo(targetVersion) <= 0);
            }

            moduleMigrations = moduleMigrations
                .OrderBy(x => x.Version)
                .ToArray();

            if (targetVersion != null
                && !Equals(moduleMigrations.LastOrDefault()?.Version, targetVersion))
            {
                throw new DatabaseMigrationException($"Cannot select database migrations for module '{moduleName}': migration for required version {targetVersion} was not found");
            }

            var version = queuedMigrations
                .Where(x => string.Equals(x.ModuleName, moduleName, StringComparison.InvariantCultureIgnoreCase))
                .OrderByDescending(x => x.Version)
                .Select(x => x.Version)
                .FirstOrDefault();

            if (version == null)
            {
                history = history ?? await migrationProvider.GetMigrationHistoryAsync();
                var lastMigration = history
                    .Where(x => string.Equals(x.ModuleName, moduleName, StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(x => x.Version).FirstOrDefault();
                version = lastMigration?.Version;
            }

            List<IDatabaseMigration> result = new List<IDatabaseMigration>();

            if (version == null)
            {
                var baseline = moduleMigrations.FirstOrDefault(x => x.IsBaseline);
                if (baseline != null)
                {
                    result.Add(baseline);
                    result.AddRange(moduleMigrations
                        .Where(x => !x.IsRepeatable && x.Version.CompareTo(baseline.Version) > 0));
                }
                else
                {
                    result.AddRange(moduleMigrations.Where(x => !x.IsRepeatable));
                }

                result.AddRange(moduleMigrations.Where(x => x.IsRepeatable));
            }
            else
            {
                result.AddRange(moduleMigrations
                    .Where(x => !x.IsRepeatable && x.Version.CompareTo(version) > 0));

                if (result.Count > 0)
                {
                    result.AddRange(moduleMigrations.Where(x => x.IsRepeatable));
                }
            }

            await AddRequiredDependenciesAsync(result, queuedMigrations, tags);
            return result;
        }

        private async Task AddRequiredDependenciesAsync(List<IDatabaseMigration> migrations,
            List<DatabaseMigrationSpecifier> queuedMigrations, string[] tags)
        {
            for (int i = 0; i < migrations.Count; i++)
            {
                var migration = migrations[i];
                var dependencySpecs = await GetRequiredMigrationDependenciesAsync(migration, queuedMigrations);
                foreach (var dependencySpec in dependencySpecs)
                {
                    var dependencyMigrations = await DoSelectMigrationsAsync(dependencySpec.ModuleName, tags,
                        dependencySpec.Version, queuedMigrations);
                    migrations.InsertRange(i, dependencyMigrations);
                    i += dependencyMigrations.Count;
                }
            }
        }

        private async Task<List<DatabaseMigrationSpecifier>> GetRequiredMigrationDependenciesAsync(
            IDatabaseMigration migration, List<DatabaseMigrationSpecifier> queuedMigrations)
        {
            var dependencies = new List<DatabaseMigrationSpecifier>();

            history = history ?? await migrationProvider.GetMigrationHistoryAsync();
            foreach (var dependency in migration.Dependencies)
            {
                var version = queuedMigrations
                    .Where(x => string.Equals(x.ModuleName, dependency.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                    .OrderByDescending(x => x.Version)
                    .Select(x => x.Version)
                    .FirstOrDefault();

                if (version == null)
                {
                    var lastMigration = history
                        .Where(x => string.Equals(x.ModuleName, dependency.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                        .OrderByDescending(x => x.Version).FirstOrDefault();
                    version = lastMigration?.Version;
                }
                
                if (version == null || version.CompareTo(dependency.Version) < 0)
                {
                    dependencies.Add(dependency);
                }
            }

            return dependencies;
        }
    }
}