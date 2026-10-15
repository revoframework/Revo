using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Revo.IntegrationTests.TestApp.Api.Requests;
using Revo.IntegrationTests.TestApp.AsyncEvents;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.AsyncEvents
{
    /// <summary>
    /// Asynchronous event processing through the in-memory job queue (outside of the HTTP request).
    /// </summary>
    public class AsyncEventListenerTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task AsyncEventListener_ReceivesEventsThroughJobQueue()
        {
            var name = $"async {Guid.NewGuid()}";
            var recorder = host.Services.GetRequiredService<AsyncEventRecorder>();

            var response = await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(Guid.NewGuid(), name));

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            await Eventually.HoldsAsync(() => Task.FromResult(recorder.RecordedNames.Any(x => x == name)),
                "the async event listener should have processed the event");
        }
    }
}
