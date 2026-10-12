using System.Threading.Tasks;
using FluentAssertions;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Revo.IntegrationTests.TestApp.DataAccess;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Hosting
{
    /// <summary>
    /// Startup (database migrations, database initializers) and shutdown of the application.
    /// </summary>
    public class ApplicationLifecycleTests
    {
        [Fact]
        public async Task Startup_MigratesDatabaseAndRunsInitializers()
        {
            await using var host = await TestAppHost.StartAsync();

            // the read model table only exists if the migrations ran, the seed list only if the initializer did
            var seedList = await host.Client.FindListAsync(TestDatabaseInitializer.SeedListId);

            seedList.Should().NotBeNull();
            seedList.Name.Should().Be(TestDatabaseInitializer.SeedListName);
        }

        [Fact]
        public async Task Shutdown_StopsCleanly()
        {
            var host = await TestAppHost.StartAsync();
            await host.Client.GetListsAsync();

            var dispose = async () => await host.DisposeAsync();

            await dispose.Should().NotThrowAsync();
        }
    }
}
