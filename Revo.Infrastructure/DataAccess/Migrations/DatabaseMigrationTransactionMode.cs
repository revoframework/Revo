namespace Revo.Infrastructure.DataAccess.Migrations
{
    /// <summary>
    /// Specifies how the database transaction should be handled a migration.
    /// </summary>
    public enum DatabaseMigrationTransactionMode
    {
        /// <summary>
        /// Migration provider's Default, which will usually wrap all current migrations inside single transaction.
        /// </summary>
        Default,

        /// <summary>
        /// Requires an isolated transaction for this single migration (or every migration, if used as a default mode).
        /// </summary>
        Isolated,


        /// <summary>
        /// Requires that migration is run outside of any transactions.
        /// </summary>
        WithoutTransaction
    }
}