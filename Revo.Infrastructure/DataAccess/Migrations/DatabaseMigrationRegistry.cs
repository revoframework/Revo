using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationRegistry : IDatabaseMigrationRegistry
    {
        readonly HashSet<IDatabaseMigration> migrations = new HashSet<IDatabaseMigration>();

        public DatabaseMigrationRegistry()
        {
        }

        public IReadOnlyCollection<IDatabaseMigration> Migrations => migrations;

        public IEnumerable<string> SearchModules(string moduleNameWildcard)
        {
            var moduleNameRegex = new Regex("^" + Regex.Escape(moduleNameWildcard).Replace("\\?", ".").Replace("\\*", ".*") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return GetAvailableModules().Where(x => moduleNameRegex.IsMatch(x));
        }

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
                    if (module.GroupBy(x => GroupMigrationTags(x.Tags)).Any(x => x.Any(m => !m.IsRepeatable)))
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': module must either have all non-repeatable, or all repeatable migration(s)");
                    }

                    if (module.GroupBy(x => GroupMigrationTags(x.Tags)).Any(x => x.Count() > 1))
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': there can be only one migration if the module has a repeatable migration");
                    }

                    var versioned = module.FirstOrDefault(x => x.Version != null);
                    if (versioned != null)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': repeatable migrations {versioned} cannot specify version (they are re-applied only when their contents changes)");
                    }

                    var baseline = module.FirstOrDefault(x => x.IsBaseline);
                    if (baseline != null)
                    {
                        throw new DatabaseMigrationException(
                            $"Failed to validate database migrations for module '{module.Key}': {baseline} cannot be both baseline and repeatable");
                    }
                }
                else
                {
                    if (module.GroupBy(x => GroupMigrationTags(x.Tags)).Any(group => group.Count(x => x.IsBaseline) > 1))
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

        private string GroupMigrationTags(string[][] tags)
        {
            return string.Join(",",
                tags.Select(tagGroup => "[" + string.Join(",", tagGroup) + "]"));
        }
    }
}