using System.Net;
using System.Threading.Tasks;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Security
{
    /// <summary>
    /// Command authorization (pre-command filter registered for all commands, with the ASP.NET Core user context).
    /// </summary>
    public class CommandAuthorizationTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task CommandRequiringAuthentication_IsRejectedForAnonymousUser()
        {
            var response = await host.Client.PostAsync("/authenticated-only", null);

            await response.ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
        }
    }
}
