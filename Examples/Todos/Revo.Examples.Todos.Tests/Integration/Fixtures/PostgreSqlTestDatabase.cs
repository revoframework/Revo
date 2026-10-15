using System.Threading.Tasks;

namespace Revo.Examples.Todos.Tests.Integration.Fixtures
{
    /// <summary>
    /// Database on the PostgreSQL server of <see cref="PostgreSqlServerFixture"/> (which gets discarded together with
    /// the whole server).
    /// </summary>
    public sealed class PostgreSqlTestDatabase(string connectionString) : ITestDatabase
    {
        public string Provider => "PostgreSQL";
        public string EFCoreProviderName => "Npgsql.EntityFrameworkCore.PostgreSQL";
        public string ConnectionString { get; } = connectionString;

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
