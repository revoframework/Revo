using System.Collections.Generic;
using System.Linq;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationRegistry : IDatabaseMigrationRegistry
    {
        readonly HashSet<IDatabaseMigration> migrations = new HashSet<IDatabaseMigration>();

        public DatabaseMigrationRegistry()
        {
        }

        public IReadOnlyCollection<IDatabaseMigration> Migrations => migrations;

        public void AddMigration(IDatabaseMigration migration)
        {
            if (!migrations.Contains(migration))
            {
                migrations.Add(migration);
            }
        }

        public IEnumerable<string> GetAvailableModules()
        {
            return Migrations.Select(x => x.ModuleName).Distinct();
        }

        public void ValidateMigrations()
        {
            var modules = migrations.GroupBy(x => x.ModuleName);
            foreach (var module in modules)
            {
                if (module.Any(x => x.IsRepeatable))
                {
                    if (module.Count() > 1)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': there can be only one migration if the module has a repeatable migration");
                    }

                    if (module.First().Version != null)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': repeatable migrations {module.First()} cannot specify version (they are re-applied only when their contents changes)");
                    }

                    if (module.First().IsBaseline)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': {module.First()} cannot be both baseline and repeatable");
                    }
                }
                else
                {
                    if (module.Count(x => x.IsBaseline) > 1)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': there cannot be more than one baseline migration");
                    }

                    var nonVersioned = module.FirstOrDefault(x => x.Version == null);
                    if (nonVersioned != null)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': {nonVersioned} must have a version");
                    }

                    var multipleVersions = module.GroupBy(x => x.Version).Where(x => x.Count() > 1);
                    foreach (var versionMigrations in multipleVersions)
                    {
                        if (versionMigrations.Any(x =>
                            versionMigrations.Any(y => x != y && x.IsBaseline == y.IsBaseline
                                                              && x.Tags.Length == y.Tags.Length
                                                              && x.Tags.All(xTagGroup => y.Tags.Any(yTagGroup =>
                                                                  xTagGroup.Length == yTagGroup.Length &&
                                                                  xTagGroup.All(yTagGroup.Contains))))))
                        {
                            throw new DatabaseMigrationException(
                                $"Failed to validate database migrations for module '{module.Key}': there are duplicate definitions for version {versionMigrations.Key}");
                        }
                    }
                }

                // TODO check for cyclic dependencies
            }
        }
    }
}