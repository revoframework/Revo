using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Revo.IntegrationTests.Fixtures
{
    /// <summary>
    /// Makes SQLite connections wait for a lock held by another connection instead of failing right away with
    /// "database is locked" - SQLite only allows a single writer at a time and the asynchronous event processing
    /// in the background writes concurrently with the requests.
    /// </summary>
    public class SqliteBusyTimeoutInterceptor : DbConnectionInterceptor
    {
        private const string BusyTimeoutPragma = "PRAGMA busy_timeout = 10000;";

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            using var command = connection.CreateCommand();
            command.CommandText = BusyTimeoutPragma;
            command.ExecuteNonQuery();
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = BusyTimeoutPragma;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
