using System;
using Microsoft.AspNetCore.Http;
using Revo.Core.Lifecycle;

namespace Revo.AspNetCore.Core;

/// <summary>
/// Ties the request lifetime to the current HTTP request.
/// </summary>
public class AspNetCoreRequestLifetime(IHttpContextAccessor httpContextAccessor) : IRequestLifetime
{
    public bool TryRegisterForDispose(IDisposable disposable)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        httpContext.Response.RegisterForDispose(disposable);
        return true;
    }
}
