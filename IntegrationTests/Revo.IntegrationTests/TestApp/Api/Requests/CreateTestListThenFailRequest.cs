using System;

namespace Revo.IntegrationTests.TestApp.Api.Requests
{
    public record CreateTestListThenFailRequest(Guid Id, Guid NestedId);
}
