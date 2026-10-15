using System;

namespace Revo.IntegrationTests.TestApp.Api.Requests
{
    public record CreateTestListWithChildrenRequest(Guid Id, string Name, Guid[] ChildIds);
}
