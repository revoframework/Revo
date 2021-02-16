using System.Collections.Generic;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationsConfiguration : IDatabaseMigrationsConfiguration
    {
        public List<DatabaseMigrationSearchSpecifier> MigrateOnlySpecifiedModules { get; set; }
        public bool? ApplyMigrationsUponStartup { get; set; }
        public string[] EnvironmentTags { get; set; } = new string[0];
        public IDatabaseMigrationScripter OverrideDatabaseMigrationScripter { get; set; }
        public DatabaseMigrationSelectorOptions MigrationSelectorOptions { get; set; } =
            new DatabaseMigrationSelectorOptions();

        IReadOnlyCollection<DatabaseMigrationSearchSpecifier> IDatabaseMigrationExecutionOptions.
            MigrateOnlySpecifiedModules => MigrateOnlySpecifiedModules;
    }
}