using System;
using Microsoft.AspNetCore.Http;
using Revo.Core.Lifecycle;

namespace Revo.AspNetCore.Core;

/// <summary>
/// Ties the request lifetime to the current HTTP request.
/// </summary>
/// <remarks>
/// The registrations are serialized, because they can come from multiple threads of the same request at once
/// (e.g. from the synchronous event queues processed in parallel, see <c>SyncQueueProcessingParallelism</c>),
/// while <see cref="HttpContext"/> is not thread-safe. The lock is shared by all requests - registrations are short
/// and infrequent (about one per DbContext), so it does not cause contention.
/// </remarks>
public class AspNetCoreRequestLifetime(IHttpContextAccessor httpContextAccessor) : IRequestLifetime
{
    private readonly object registrationLock = new();

    public bool TryRegisterForDispose(IDisposable disposable)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        lock (registrationLock)
        {
            httpContext.Response.RegisterForDispose(disposable);
        }

        return true;
    }
}
