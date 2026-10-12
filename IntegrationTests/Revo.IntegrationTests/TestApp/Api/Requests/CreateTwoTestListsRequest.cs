using System;

namespace Revo.IntegrationTests.TestApp.Api.Requests
{
    public record CreateTwoTestListsRequest(Guid FirstId, Guid SecondId, string Name);
}
