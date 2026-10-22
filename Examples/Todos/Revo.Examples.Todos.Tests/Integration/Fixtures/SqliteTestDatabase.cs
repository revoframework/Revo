using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Revo.Examples.Todos.Tests.Integration.Fixtures
{
    /// <summary>
    /// SQLite database in a temporary file.
    /// </summary>
    public sealed class SqliteTestDatabase : ITestDatabase
    {
        private readonly string databasePath =
            Path.Combine(Path.GetTempPath(), $"revo-todos-tests-{Guid.NewGuid():N}.db");

        public string Provider => "Sqlite";
        public string EFCoreProviderName => "Microsoft.EntityFrameworkCore.Sqlite";
        public string ConnectionString => $"Data Source={databasePath};Pooling=False";

        public ValueTask DisposeAsync()
        {
            SqliteConnection.ClearAllPools();
            foreach (var path in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" })
            {
                File.Delete(path);
            }

            return ValueTask.CompletedTask;
        }
    }
}
