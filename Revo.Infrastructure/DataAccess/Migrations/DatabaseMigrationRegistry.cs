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
                if (module.Count(x => x.IsBaseline) > 1)
                {
                    throw new DatabaseMigrationException(
                        $"Failed to validate database migrations for module '{module.Key}': there cannot be more than one baseline migration");
                }

                /*var multipleVersions = module.GroupBy(x => x.Version).Where(x => x.Count() > 1);
                foreach (var duplicate in multipleVersions)
                {
                    throw new DatabaseMigrationException(
                        $"Failed to validate database migrations for module '{module.Key}': there are duplicate definitions for version {duplicate.Key}");
                }*/

                var baselineAndRepeatable = module.FirstOrDefault(x => x.IsBaseline && x.IsRepeatable);
                if (baselineAndRepeatable != null)
                {
                    throw new DatabaseMigrationException(
                        $"Failed to validate database migrations for module '{module.Key}': version {baselineAndRepeatable.Version} migration cannot be both baseline and repeatable");
                }

                // TODO check for cyclic dependencies
            }
        }
    }
}