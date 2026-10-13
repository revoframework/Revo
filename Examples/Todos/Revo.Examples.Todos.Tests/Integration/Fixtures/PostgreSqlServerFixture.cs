using System;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Revo.Examples.Todos.Tests.Integration.Fixtures
{
    /// <summary>
    /// Disposable PostgreSQL server running in a Docker container, shared by the tests of a test class (each of them
    /// gets its own database on it).
    /// </summary>
    /// <remarks>
    /// When Docker is not available, the tests using it get skipped - except on a CI server, where they fail,
    /// so that they cannot silently stop running.
    /// </remarks>
    public sealed class PostgreSqlServerFixture : IAsyncLifetime
    {
        private const string Image = "postgres:17-alpine";

        private PostgreSqlContainer container;
        private string dockerUnavailableReason;

        public async Task InitializeAsync()
        {
            try
            {
                container = new PostgreSqlBuilder(Image).Build();
                await container.StartAsync();
            }
            catch (DockerUnavailableException e) when (!IsRunningOnCiServer())
            {
                container = null;
                dockerUnavailableReason = e.Message;
            }
        }

        public async Task DisposeAsync()
        {
            if (container != null)
            {
                await container.DisposeAsync();
            }
        }

        /// <summary>
        /// Creates a new empty database (skips the calling test if Docker is not available).
        /// </summary>
        public async Task<ITestDatabase> CreateDatabaseAsync()
        {
            Skip.If(container == null, $"Docker is not available: {dockerUnavailableReason}");

            var databaseName = $"todos_{Guid.NewGuid():N}";
            await using (var connection = new NpgsqlConnection(container.GetConnectionString()))
            {
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
                await command.ExecuteNonQueryAsync();
            }

            var connectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString())
            {
                Database = databaseName
            };

            return new PostgreSqlTestDatabase(connectionString.ConnectionString);
        }

        private static bool IsRunningOnCiServer()
        {
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")) // Azure Pipelines
                   || string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
