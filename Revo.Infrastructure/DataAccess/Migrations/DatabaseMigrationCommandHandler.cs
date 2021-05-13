using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.DataAccess.Migrations
{
    public class DatabaseMigrationCommandHandler :
        ICommandHandler<ExecuteDatabaseMigrationsCommand>
    {
        private readonly IDatabaseMigrationExecutor executor;

        public DatabaseMigrationCommandHandler(IDatabaseMigrationExecutor executor)
        {
            this.executor = executor;
        }

        public async Task HandleAsync(ExecuteDatabaseMigrationsCommand command, CancellationToken cancellationToken)
        {
            // we execute migrations in a command so that applications can hook database migration events
            // and perform stuff while inside the same transaction
            await executor.ExecuteAsync();
        }
    }
}