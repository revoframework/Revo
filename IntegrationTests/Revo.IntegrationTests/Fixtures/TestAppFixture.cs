using System.Threading.Tasks;
using Xunit;

namespace Revo.IntegrationTests.Fixtures
{
    /// <summary>
    /// Shares one running test application among the tests of a test class.
    /// </summary>
    public sealed class TestAppFixture : IAsyncLifetime
    {
        public TestAppHost Host { get; private set; }

        public async Task InitializeAsync()
        {
            Host = await TestAppHost.StartAsync();
        }

        public async Task DisposeAsync()
        {
            if (Host != null)
            {
                await Host.DisposeAsync();
            }
        }
    }
}
