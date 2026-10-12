using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.IntegrationTests.Fixtures;
using Revo.IntegrationTests.Support;
using Revo.IntegrationTests.TestApp.Api.Requests;
using Xunit;

namespace Revo.IntegrationTests.Scenarios.EFCore
{
    /// <summary>
    /// Event-sourced aggregates persisted by commands and projected to EF Core read models
    /// (projections are dispatched synchronously, i.e. before the command returns).
    /// </summary>
    public class ProjectionTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task CreateCommand_IsPersistedAndProjectedSynchronously()
        {
            var id = Guid.NewGuid();

            var response = await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(id, "Groceries"));

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            (await host.Client.FindListAsync(id)).Should().BeEquivalentTo(new TestListDto(id, "Groceries", 1));
        }

        [Fact]
        public async Task SubsequentCommand_UpdatesTheSameAggregate()
        {
            var id = Guid.NewGuid();
            await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(id, "Groceries"));

            var response = await host.Client.PutAsJsonAsync($"/lists/{id}",
                new RenameTestListRequest("Weekly groceries"));

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            (await host.Client.FindListAsync(id)).Should().BeEquivalentTo(
                new TestListDto(id, "Weekly groceries", 2));
        }
    }
}
