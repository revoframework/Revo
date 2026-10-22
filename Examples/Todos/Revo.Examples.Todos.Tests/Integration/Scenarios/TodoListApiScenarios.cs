using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.EFCore.DataAccess.Entities;
using Revo.Examples.Todos.Tests.Integration.Fixtures;
using Revo.Examples.Todos.Tests.Integration.Support;
using Xunit;

namespace Revo.Examples.Todos.Tests.Integration.Scenarios
{
    /// <summary>
    /// Runs the whole Todos app through its HTTP API, against the database provided by the derived class
    /// (every test gets a new empty database).
    /// </summary>
    public abstract class TodoListApiScenarios : IAsyncLifetime
    {
        private ITestDatabase database;

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (database != null)
            {
                await database.DisposeAsync();
            }
        }

        [SkippableFact]
        public async Task CreatedTodoList_IsListed()
        {
            await using var app = await StartAppAsync();
            var api = new TodosApiClient(app.CreateClient());
            var id = Guid.NewGuid();

            await api.CreateTodoListAsync(id, "Groceries");

            var todoList = await api.FindTodoListAsync(id);
            todoList.Should().NotBeNull();
            todoList.Name.Should().Be("Groceries");
            todoList.Todos.Should().BeEmpty();
        }

        [SkippableFact]
        public async Task RenamedTodoList_HasNewName()
        {
            await using var app = await StartAppAsync();
            var api = new TodosApiClient(app.CreateClient());
            var id = Guid.NewGuid();
            await api.CreateTodoListAsync(id, "Groceries");

            await api.RenameTodoListAsync(id, "Weekly groceries");

            (await api.FindTodoListAsync(id)).Name.Should().Be("Weekly groceries");
        }

        [SkippableFact]
        public async Task AddedTodos_AreListed()
        {
            await using var app = await StartAppAsync();
            var api = new TodosApiClient(app.CreateClient());
            var id = Guid.NewGuid();
            await api.CreateTodoListAsync(id, "Groceries");

            await api.AddTodoAsync(id, "Milk");
            await api.AddTodoAsync(id, "Bread");

            var todos = (await api.FindTodoListAsync(id)).Todos;
            todos.Select(x => x.Text).Should().BeEquivalentTo("Milk", "Bread");
            todos.Should().OnlyContain(x => x.TodoListId == id && !x.IsComplete);
        }

        [SkippableFact]
        public async Task UpdatedTodo_HasNewTextAndCompletion()
        {
            await using var app = await StartAppAsync();
            var api = new TodosApiClient(app.CreateClient());
            var id = Guid.NewGuid();
            await api.CreateTodoListAsync(id, "Groceries");
            await api.AddTodoAsync(id, "Milk");
            var todoId = (await api.FindTodoListAsync(id)).Todos.Single().Id;

            await api.UpdateTodoAsync(id, todoId, "Milk (2 l)", isComplete: true);

            var todo = (await api.FindTodoListAsync(id)).Todos.Single();
            todo.Text.Should().Be("Milk (2 l)");
            todo.IsComplete.Should().BeTrue();
        }

        [SkippableFact]
        public async Task TodoLists_SurviveApplicationRestart()
        {
            // the second start also runs against an already migrated (and initialized) database
            var id = Guid.NewGuid();
            await using (var app = await StartAppAsync())
            {
                var api = new TodosApiClient(app.CreateClient());
                await api.CreateTodoListAsync(id, "Groceries");
                await api.AddTodoAsync(id, "Milk");
            }

            await using (var restartedApp = await StartAppAsync())
            {
                var api = new TodosApiClient(restartedApp.CreateClient());

                var todoList = await api.FindTodoListAsync(id);
                todoList.Name.Should().Be("Groceries");
                todoList.Todos.Select(x => x.Text).Should().Equal("Milk");
            }
        }

        /// <summary>
        /// Creates the database (skipping the test if it is not available).
        /// </summary>
        protected abstract Task<ITestDatabase> CreateDatabaseAsync();

        /// <summary>
        /// Starts a new instance of the Todos app on the test's database (created upon the first start).
        /// </summary>
        protected async Task<TodosAppFactory> StartAppAsync()
        {
            database ??= await CreateDatabaseAsync();
            var app = new TodosAppFactory(database);

            // guards against the app silently running on a different database than the one under test
            using var dbContext = app.Services.GetRequiredService<IDbContextFactory>()
                .CreateContext(EFCoreDatabaseAccess.DefaultSchemaSpace);
            dbContext.Database.ProviderName.Should().Be(database.EFCoreProviderName);

            return app;
        }
    }
}
