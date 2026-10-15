using System.Threading.Tasks;
using Revo.Examples.Todos.Tests.Integration.Fixtures;
using Xunit;

namespace Revo.Examples.Todos.Tests.Integration.Scenarios
{
    public class PostgreSqlTodoListApiTests(PostgreSqlServerFixture postgreSqlServer)
        : TodoListApiScenarios, IClassFixture<PostgreSqlServerFixture>
    {
        protected override Task<ITestDatabase> CreateDatabaseAsync()
        {
            return postgreSqlServer.CreateDatabaseAsync();
        }
    }
}
