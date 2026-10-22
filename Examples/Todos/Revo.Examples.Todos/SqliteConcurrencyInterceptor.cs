using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Revo.Examples.Todos
{
    /// <summary>
    /// Makes SQLite work with concurrent writers: SQLite only allows a single writer at a time and the asynchronous
    /// event processing in the background writes concurrently with the requests. Instead of failing right away
    /// with "database is locked", connections wait for the lock (busy timeout); WAL mode lets readers and the writer
    /// work without blocking each other.
    /// </summary>
    public class SqliteConcurrencyInterceptor : DbConnectionInterceptor
    {
        private const string Pragmas = "PRAGMA busy_timeout = 10000; PRAGMA journal_mode = WAL;";

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            using var command = connection.CreateCommand();
            command.CommandText = Pragmas;
            command.ExecuteNonQuery();
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = Pragmas;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
