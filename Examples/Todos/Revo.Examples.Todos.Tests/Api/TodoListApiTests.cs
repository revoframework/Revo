using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Xunit;
using Xunit.Sdk;

namespace Revo.Examples.Todos.Tests.Api
{
    /// <summary>
    /// Runs the whole example application (in-memory, on a temporary SQLite database) through its HTTP API.
    /// </summary>
    public class TodoListApiTests : IAsyncLifetime
    {
        private readonly string databasePath =
            Path.Combine(Path.GetTempPath(), $"revo-todos-tests-{Guid.NewGuid():N}.db");

        private WebApplicationFactory<Program> factory;

        public Task InitializeAsync()
        {
            factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting("ConnectionStrings:Sqlite", $"Data Source={databasePath};Pooling=False");
            });

            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await factory.DisposeAsync();
            SqliteConnection.ClearAllPools();
            foreach (var path in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" })
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task TodoList_IsCreatedUpdatedAndRead()
        {
            var client = factory.CreateClient();
            var id = Guid.NewGuid();

            await EnsureSuccessAsync(await client.PostAsJsonAsync("/api/todo-lists",
                new { id, name = "Groceries" }));
            await EnsureSuccessAsync(await client.PutAsJsonAsync($"/api/todo-lists/{id}",
                new { name = "Weekly groceries" }));
            await EnsureSuccessAsync(await client.PostAsJsonAsync($"/api/todo-lists/{id}",
                new { text = "Milk" }));

            var lists = await client.GetFromJsonAsync<JsonElement[]>("/api/todo-lists");

            var list = lists.Should().ContainSingle(x => x.GetProperty("id").GetGuid() == id).Which;
            list.GetProperty("name").GetString().Should().Be("Weekly groceries");
            list.GetProperty("version").GetInt32().Should().Be(3);
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new XunitException(
                    $"Request failed with {(int) response.StatusCode}:\n{await response.Content.ReadAsStringAsync()}");
            }
        }
    }
}
