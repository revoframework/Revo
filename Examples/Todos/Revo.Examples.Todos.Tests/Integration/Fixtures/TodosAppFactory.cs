using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Revo.Examples.Todos.Tests.Integration.Fixtures
{
    /// <summary>
    /// Runs the whole Todos app in-memory against the given database.
    /// </summary>
    public class TodosAppFactory(ITestDatabase database) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("DatabaseProvider", database.Provider);
            builder.UseSetting($"ConnectionStrings:{database.Provider}", database.ConnectionString);
        }
    }
}
