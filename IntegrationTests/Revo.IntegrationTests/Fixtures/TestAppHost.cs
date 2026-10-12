using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Revo.IntegrationTests.TestApp;

namespace Revo.IntegrationTests.Fixtures
{
    /// <summary>
    /// Runs the test application in-memory (<see cref="TestServer"/>) on a new SQLite database in a temporary
    /// file, with the service provider validation of the Development environment.
    /// </summary>
    public sealed class TestAppHost : IAsyncDisposable
    {
        private readonly string databasePath;

        private TestAppHost(WebApplication app, string databasePath)
        {
            App = app;
            this.databasePath = databasePath;
            Client = app.GetTestClient();
        }

        public WebApplication App { get; }
        public HttpClient Client { get; }
        public IServiceProvider Services => App.Services;

        public static async Task<TestAppHost> StartAsync()
        {
            var databasePath = Path.Combine(Path.GetTempPath(), $"revo-integration-tests-{Guid.NewGuid():N}.db");
            var connectionString = $"Data Source={databasePath};Pooling=False";
            await EnableWriteAheadLoggingAsync(connectionString);

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = Environments.Development
            });

            builder.WebHost.UseTestServer();
            builder.Host.UseDefaultServiceProvider(options =>
            {
                options.ValidateOnBuild = true;
                options.ValidateScopes = true;
            });

            builder.Services.AddTestApp(contextBuilder => contextBuilder
                .UseSqlite(connectionString)
                .AddInterceptors(new SqliteBusyTimeoutInterceptor()));

            WebApplication app = null;
            try
            {
                app = builder.Build();
                app.UseTestApp();

                await app.StartAsync();
                return new TestAppHost(app, databasePath);
            }
            catch
            {
                if (app != null)
                {
                    await app.DisposeAsync();
                }

                DeleteDatabase(databasePath);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
            DeleteDatabase(databasePath);
        }

        /// <summary>
        /// Creates the database in the WAL mode, in which readers do not block the writer (and vice versa).
        /// </summary>
        private static async Task EnableWriteAheadLoggingAsync(string connectionString)
        {
            await using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode = WAL;";
            await command.ExecuteNonQueryAsync();
        }

        private static void DeleteDatabase(string databasePath)
        {
            SqliteConnection.ClearAllPools();
            foreach (var path in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" })
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                {
                    // best effort only, these are temp files anyway
                }
            }
        }
    }
}
