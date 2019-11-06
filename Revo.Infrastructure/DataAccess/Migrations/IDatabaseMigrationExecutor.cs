using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public interface IDatabaseMigrationExecutor
    {
        /// <summary>
        /// Executes all pending migrations.
        /// </summary>
        /// <returns>Migrations applied.</returns>
        Task<IReadOnlyCollection<PendingModuleMigration>> ExecuteAsync();

        /// <summary>
        /// Previews all pending migrations.
        /// </summary>
        /// <returns>Migrations that would be applied with ExecuteAsync.</returns>
        Task<IReadOnlyCollection<PendingModuleMigration>> PreviewAsync();
    }
}