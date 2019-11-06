using System.Collections.Generic;
using MoreLinq.Extensions;
using Revo.Infrastructure.DataAccess.Migrations.Providers;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationsConfiguration : IDatabaseMigrationsConfiguration
    {
        public HashSet<DatabaseMigrationSpecifier> MigrateOnlySpecifiedModules { get; set; }
        public bool? ApplyMigrationsUponStartup { get; set; }
        public string[] EnvironmentTags { get; set; } = new string[0];
        public IDatabaseMigrationScripter OverrideDatabaseMigrationScripter { get; set; }

        IReadOnlyCollection<DatabaseMigrationSpecifier> IDatabaseMigrationExecutionOptions.
            MigrateOnlySpecifiedModules => MigrateOnlySpecifiedModules;
    }
}