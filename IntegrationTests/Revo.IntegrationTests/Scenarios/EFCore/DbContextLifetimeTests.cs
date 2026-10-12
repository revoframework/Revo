using System;
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
    /// DbContexts created by a command outlive its service scope until the end of the HTTP request
    /// (request lifetime of the ASP.NET Core integration).
    /// </summary>
    public class DbContextLifetimeTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task QueryReturningIQueryable_IsEvaluatedAfterItsCommandScopeEnds()
        {
            // GET /lists returns an IQueryable that is only enumerated when serializing the response - after the
            // query's command scope has been disposed
            var id = Guid.NewGuid();
            await host.Client.PostAsJsonAsync("/lists", new CreateTestListRequest(id, "Lazily read"));

            var lists = await host.Client.GetListsAsync();

            lists.Should().Contain(x => x.Id == id && x.Name == "Lazily read");
        }
    }
}
