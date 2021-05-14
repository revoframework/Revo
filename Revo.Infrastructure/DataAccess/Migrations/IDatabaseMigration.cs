using System.Collections.Generic;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigration
    {
        /// <summary>
        /// Name of the application module this migration belongs to.
        /// Can be any user-defined name, e.g. 'myapp_views'.
        /// </summary>
        string ModuleName { get; }

        /// <summary>
        /// Baseline migrations are only run on empty databases and skip any preceding
        /// version migrations (instead of running all migrations sequentially, only one
        /// baseline migration is run, which is expected to fully create the database from scratch).
        /// </summary>
        bool IsBaseline { get; }

        /// <summary>
        /// Repeatable migrations are run after every version upgrade or downgrade
        /// (suitable for recreating views, etc.).
        /// They are not applied sequentially and always only the latest (or unversioned,
        /// which is expected to always be of the latest version) is run.
        /// </summary>
        bool IsRepeatable { get; }

        /// <summary>
        /// Destination version of the migration.
        /// </summary>
        DatabaseVersion Version { get; }

        /// <summary>
        /// Dependencies of this migration.
        /// Specified modules must be at provided versions before running this migration.
        /// </summary>
        IReadOnlyCollection<DatabaseMigrationSpecifier> Dependencies { get; }

        /// <summary>
        /// Tags specifying the target environment (e.g. development/production) for this migration.
        /// From every group of the tags, the environment must match at least 1 tag.
        /// </summary>
        string[][] Tags { get; }

        /// <summary>
        /// Specifies how the database transaction should be handled for this migration.
        /// Defaults to provider's Default, which will usually wrap all current migrations inside single transaction.
        /// </summary>
        DatabaseMigrationTransactionMode TransactionMode { get; }

        string Description { get; }
        string Checksum { get; }
        string ToString(bool includeClassName);
    }
}
