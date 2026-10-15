using System;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Request lifetime used when there is no notion of requests (e.g. without ASP.NET Core integration).
/// </summary>
public class NullRequestLifetime : IRequestLifetime
{
    public bool TryRegisterForDispose(IDisposable disposable)
    {
        return false;
    }
}
