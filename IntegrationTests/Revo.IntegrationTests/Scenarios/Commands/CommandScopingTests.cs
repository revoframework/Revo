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
    /// Every command (incl. the nested ones) runs in its own service scope with its own repository
    /// and unit of work.
    /// </summary>
    public class CommandScopingTests(TestAppFixture fixture) : IClassFixture<TestAppFixture>
    {
        private readonly TestAppHost host = fixture.Host;

        [Fact]
        public async Task TwoCommandsInOneRequest_AreBothPersisted()
        {
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();

            var response = await host.Client.PostAsJsonAsync("/lists/two-in-one-request",
                new CreateTwoTestListsRequest(firstId, secondId, "Twin"));

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            (await host.Client.FindListAsync(firstId)).Should().NotBeNull();
            (await host.Client.FindListAsync(secondId)).Should().NotBeNull();
        }

        [Fact]
        public async Task NestedCommands_AreAllPersisted()
        {
            var id = Guid.NewGuid();
            var childIds = new[] { Guid.NewGuid(), Guid.NewGuid() };

            var response = await host.Client.PostAsJsonAsync("/lists/with-children",
                new CreateTestListWithChildrenRequest(id, "Parent", childIds));

            await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
            (await host.Client.FindListAsync(id)).Should().NotBeNull();
            (await host.Client.FindListAsync(childIds[0])).Should().NotBeNull();
            (await host.Client.FindListAsync(childIds[1])).Should().NotBeNull();
        }

        [Fact]
        public async Task FailedCommand_IsNotPersisted_ButItsCompletedNestedCommandIs()
        {
            // the nested command's unit of work gets committed independently of the outer command
            // (as it did with Ninject's InTaskScope)
            var id = Guid.NewGuid();
            var nestedId = Guid.NewGuid();

            var response = await host.Client.PostAsJsonAsync("/lists/then-fail",
                new CreateTestListThenFailRequest(id, nestedId));

            await response.ShouldHaveStatusAsync(HttpStatusCode.InternalServerError);
            (await host.Client.FindListAsync(id)).Should().BeNull();
            (await host.Client.FindListAsync(nestedId)).Should().NotBeNull();
        }
    }
}
