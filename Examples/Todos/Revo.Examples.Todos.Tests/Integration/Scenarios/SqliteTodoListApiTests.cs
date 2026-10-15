using System.Threading.Tasks;
using Revo.Examples.Todos.Tests.Integration.Fixtures;

namespace Revo.Examples.Todos.Tests.Integration.Scenarios
{
    public class SqliteTodoListApiTests : TodoListApiScenarios
    {
        protected override Task<ITestDatabase> CreateDatabaseAsync()
        {
            return Task.FromResult<ITestDatabase>(new SqliteTestDatabase());
        }
    }
}
