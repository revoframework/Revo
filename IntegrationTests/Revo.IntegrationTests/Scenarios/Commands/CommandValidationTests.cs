using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Revo.IntegrationTests.TestApp.Api.Requests;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Commands
{
    /// <summary>
    /// Validation of command attributes (pre-command filter registered for all commands).
    /// </summary>
    public class CommandValidationTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task InvalidCommand_IsRejected()
        {
            var id = Guid.NewGuid();

            var response = await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(id, null));

            await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
            (await host.Client.FindListAsync(id)).Should().BeNull();
        }
    }
}
