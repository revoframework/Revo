using System.Collections.Generic;
using Revo.Infrastructure.DataAccess.Migrations;

namespace Revo.Tools.DatabaseMigrator
{
    public class DatabaseMigrationExecutionOptions : IDatabaseMigrationExecutionOptions
    {
        public IReadOnlyCollection<DatabaseMigrationSearchSpecifier> MigrateOnlySpecifiedModules { get; set; }
        public string[] EnvironmentTags { get; set; }
        public DatabaseMigrationSelectorOptions MigrationSelectorOptions { get; set; } =
            new DatabaseMigrationSelectorOptions();
    }
}