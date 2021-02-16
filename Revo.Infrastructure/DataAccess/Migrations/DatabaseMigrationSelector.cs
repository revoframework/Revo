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
        private readonly IDatabaseMigrationSelectorOptions selectorOptions;
        private IReadOnlyCollection<IDatabaseMigrationRecord> history;

        public DatabaseMigrationSelector(IDatabaseMigrationRegistry migrationRegistry, IDatabaseMigrationProvider migrationProvider,
            IDatabaseMigrationSelectorOptions selectorOptions)
        {
            this.migrationRegistry = migrationRegistry;
            this.migrationProvider = migrationProvider;
            this.selectorOptions = selectorOptions;
        }
        
        public async Task<IReadOnlyCollection<SelectedModuleMigrations>> SelectMigrationsAsync(DatabaseMigrationSpecifier[] modules, string[] tags)
        {
            try
            {
                var queuedMigrations = new List<DatabaseMigrationSpecifier>();
                var result = new List<SelectedModuleMigrations>();
                history = await migrationProvider.GetMigrationHistoryAsync();

                // select migrations for modules, repeatable and unversioned last
                var sortedModules = modules.OrderBy(x => IsRepeatableMigration(x) ? 1 : 0);

                foreach (var module in sortedModules)
                {
                    var migrations = await DoSelectMigrationsAsync(module, tags, queuedMigrations);

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

        private bool IsRepeatableMigration(DatabaseMigrationSpecifier specifier)
        {
            var firstMigration = migrationRegistry.Migrations
                .FirstOrDefault(x => string.Equals(x.ModuleName, specifier.ModuleName,
                    StringComparison.InvariantCultureIgnoreCase));
            return firstMigration != null && firstMigration.IsRepeatable;
        }

        private async Task<IReadOnlyCollection<IDatabaseMigration>> DoSelectMigrationsAsync(DatabaseMigrationSpecifier specifier, string[] tags,
            List<DatabaseMigrationSpecifier> queuedMigrations)
        {
            var result = await SelectModuleMigrationsNoDependenciesAsync(specifier, tags, queuedMigrations);

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

                var dependencySpecs = await GetRequiredMigrationDependenciesAsync(migration, queuedMigrations, tags);
                foreach (var dependencySpec in dependencySpecs)
                {
                    var dependencyMigrations = await DoSelectMigrationsAsync(dependencySpec, tags, queuedMigrations);
                    migrations.InsertRange(i, dependencyMigrations);
                    queuedMigrations.AddRange(dependencyMigrations.Select(x => new DatabaseMigrationSpecifier(x.ModuleName, x.Version)));
                    i += dependencyMigrations.Count;
                }
            }
        }

        private async Task<List<DatabaseMigrationSpecifier>> GetRequiredMigrationDependenciesAsync(
            IDatabaseMigration migration, List<DatabaseMigrationSpecifier> queuedMigrations, string[] tags)
        {
            var dependencies = new List<DatabaseMigrationSpecifier>();
            
            foreach (var dependency in migration.Dependencies)
            {
                var dependencyMigrations = await SelectModuleMigrationsNoDependenciesAsync(dependency, tags, queuedMigrations);
                if (dependencyMigrations.Count > 0)
                {
                    dependencies.Add(new DatabaseMigrationSpecifier(
                        dependencyMigrations.Last().ModuleName,
                        dependencyMigrations.Last().Version));
                }
            }

            return dependencies;
        }

        private async Task<List<IDatabaseMigration>> SelectModuleMigrationsNoDependenciesAsync(DatabaseMigrationSpecifier specifier,
            string[] tags, List<DatabaseMigrationSpecifier> queuedMigrations)
        {
            var moduleMigrations = migrationRegistry.Migrations
                .Where(x => string.Equals(x.ModuleName, specifier.ModuleName, StringComparison.InvariantCultureIgnoreCase))
                .Where(x => x.Tags.All(tagGroup => tags.Any(tagGroup.Contains)));

            if (specifier.Version != null)
            {
                moduleMigrations = moduleMigrations.Where(x => x.Version.CompareTo(specifier.Version) <= 0);
            }

            moduleMigrations = moduleMigrations
                .OrderBy(x => x.Version)
                .ToArray();

            var historyMigrations = history
                .Where(x => string.Equals(x.ModuleName, specifier.ModuleName, StringComparison.InvariantCultureIgnoreCase));
            var moduleQueuedMigrations = queuedMigrations
                .Where(x => string.Equals(x.ModuleName, specifier.ModuleName, StringComparison.InvariantCultureIgnoreCase));

            if (specifier.Version != null)
            {
                if (moduleMigrations.Any() && moduleMigrations.Last().IsRepeatable)
                {
                    throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier} because it is a repeatable migration module, which means it is versioned only by checksums");
                }
            }
            else if (!moduleMigrations.Any())
            {
                if (!historyMigrations.Any())
                {
                    // TODO maybe return without errors if there are migrations for this module with different tags?
                    throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier}: no migrations for specified module were found");
                }

                return new List<IDatabaseMigration>();
            }

            if (historyMigrations.Any() && moduleMigrations.Any())
            {
                bool wasRepeatable = historyMigrations.First().Version == null;
                bool isRepeatable = moduleMigrations.First().IsRepeatable;

                if (wasRepeatable != isRepeatable)
                {
                    if (wasRepeatable)
                    {
                        throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier}: previously was module used as repeatable, now is versioned");
                    }
                    else
                    {
                        throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier}: previously was module used as versioned, now is repeatable");
                    }
                }
            }

            // repeatable-migration modules
            if (moduleMigrations.Any() && moduleMigrations.First().IsRepeatable)
            {
                if (moduleQueuedMigrations.Any())
                {
                    return new List<IDatabaseMigration>();
                }

                var lastMigration = historyMigrations
                    .OrderByDescending(x => x.TimeApplied)
                    .FirstOrDefault();

                var migration = moduleMigrations.SingleOrDefault()
                    ?? throw new DatabaseMigrationException($"Cannot select database migrations for repeatable module {specifier}: multiple migrations found");

                // return if same and no dependencies got updated
                if (lastMigration?.Checksum == migration.Checksum)
                {
                    if (!selectorOptions.RerunRepeatableMigrationsOnDependencyUpdate
                        || !migration.Dependencies.Any(dep => dep.Version == null /* either 'latest' or repeatable */
                                                              && queuedMigrations.Any(queued =>
                                                                  string.Equals(queued.ModuleName, dep.ModuleName,
                                                                      StringComparison.InvariantCultureIgnoreCase))))
                    {
                        return new List<IDatabaseMigration>();
                    }
                }

                return new List<IDatabaseMigration>() {migration};
            }
            // versioned modules
            else
            {
                var version = moduleQueuedMigrations
                    .OrderByDescending(x => x.Version)
                    .Select(x => x.Version)
                    .FirstOrDefault();

                if (version == null)
                {
                    var lastMigration = historyMigrations
                        .OrderByDescending(x => x.Version).FirstOrDefault();
                    version = lastMigration?.Version;
                }

                var result = new List<IDatabaseMigration>();

                if (version == null)
                {
                    var baseline = moduleMigrations.FirstOrDefault(x => x.IsBaseline);
                    if (baseline != null)
                    {
                        result.Add(baseline);
                        result.AddRange(moduleMigrations
                            .Where(x => x.Version.CompareTo(baseline.Version) > 0));
                    }
                    else
                    {
                        result.AddRange(moduleMigrations);
                    }
                }
                else
                {
                    result.AddRange(moduleMigrations
                        .Where(x => !x.IsBaseline && x.Version.CompareTo(version) > 0));
                }

                if (specifier.Version != null)
                {
                    if ((version == null || version.CompareTo(specifier.Version) < 0)
                        && (!result.Any() || !Equals(result.LastOrDefault()?.Version, specifier.Version)))
                    {
                        throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier}: migration for required version {specifier.Version} from {version?.ToString() ?? "nothing"} was not found");
                    }
                }
                else
                {
                    var maxVersion = moduleMigrations.Select(x => x.Version).Max();

                    if ((!result.Any() && (version == null || version.CompareTo(maxVersion) < 0))
                        || (result.Any() && !result.Last().Version.Equals(maxVersion)))
                    {
                        throw new DatabaseMigrationException($"Cannot select database migrations for module {specifier}: migration path to required version 'latest' ({maxVersion}) from {version?.ToString() ?? "nothing"} was not found");
                    }
                }

                return result;
            }
        }
    }
}