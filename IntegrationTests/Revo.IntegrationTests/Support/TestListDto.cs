using System;

namespace Revo.IntegrationTests.Support
{
    /// <summary>
    /// List as returned by the test application's API.
    /// </summary>
    public record TestListDto(Guid Id, string Name, int Version);
}
