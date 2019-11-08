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
        
        public async Task<IReadOnlyCollection<SelectedModuleMigrations>> SelectMigrationsAsync(DatabaseMigrationSpecifier[] modules, string[] tags)
        {
            try
            {
                var queuedMigrations = new List<DatabaseMigrationSpecifier>();
                var result = new List<SelectedModuleMigrations>();

                foreach (var module in modules)
                {
                    var migrations = await DoSelectMigrationsAsync(module.ModuleName, tags, module.Version, queuedMigrations);

                    if (migrations.Count > 0)
                    {
                        result.Add(new SelectedModuleMigrations(module, migrations));
                    }
                }

                return result;
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
                    .Where(x => !x.IsRepeatable && !x.IsBaseline && x.Version.CompareTo(version) > 0));

                if (result.Count > 0)
                {
                    result.AddRange(moduleMigrations.Where(x => x.IsRepeatable));
                }
            }

            foreach (var migration in result)
            {
                queuedMigrations.Add(new DatabaseMigrationSpecifier(migration.ModuleName, migration.Version));
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
                    queuedMigrations.AddRange(dependencyMigrations.Select(x => new DatabaseMigrationSpecifier(x.ModuleName, x.Version)));
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

                var realDependencyVersion = dependency.Version
                    ?? migrationRegistry.Migrations.Where(x => x.ModuleName == dependency.ModuleName
                                                               && x.Version != null && (version == null || !x.IsBaseline)
                                                               && !x.IsRepeatable)
                        .OrderByDescending(x => x.Version)
                        .Select(x => x.Version)
                        .FirstOrDefault()
                    ?? version
                    ?? throw new DatabaseMigrationException($"Database migration {migration} specifies dependency to latest version of {dependency.ModuleName}, but no versions of that module were found");

                if (version == null || version.CompareTo(realDependencyVersion) < 0)
                {
                    dependencies.Add(dependency);
                }
            }

            return dependencies;
        }
    }
}