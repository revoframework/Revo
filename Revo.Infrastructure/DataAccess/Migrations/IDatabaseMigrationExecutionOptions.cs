using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationExecutionOptions
    {
        /// <summary>
        /// Specifies modules to migrate, possibly using wildcards. The order of these specifiers
        /// is preserved, which means you can also use this to enforce the order in which
        /// the individual modules are migrated (while their dependency constraints are still upheld).
        /// to enforce the order of migrating modules
        /// If null, migrates all found modules to their latest versions.
        /// </summary>
        IReadOnlyCollection<DatabaseMigrationSearchSpecifier> MigrateOnlySpecifiedModules { get; }
        string[] EnvironmentTags { get; }
    }
}