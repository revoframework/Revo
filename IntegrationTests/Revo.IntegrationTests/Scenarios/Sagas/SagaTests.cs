using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Revo.IntegrationTests.TestApp.Api.Requests;
using Revo.IntegrationTests.TestApp.Sagas;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.Sagas
{
    /// <summary>
    /// Sagas get the events asynchronously (in-memory job queue) and send commands in reaction to them.
    /// </summary>
    public class SagaTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task Saga_ReactsToEventBySendingCommand()
        {
            var id = Guid.NewGuid();
            await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(id, "Before saga"));

            var response = await host.Client.PostAsync($"/lists/{id}/trigger-saga", null);

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            await Eventually.HoldsAsync(
                async () => (await host.Client.FindListAsync(id))?.Name == TestListSaga.ReactionName,
                "the saga should have renamed the list");
        }
    }
}
