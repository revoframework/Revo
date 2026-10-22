using System;

namespace Revo.Core.Lifecycle;

/// <summary>
/// Lifetime of the current request (e.g. an HTTP request), which can span multiple commands, each of them executed
/// in its own service scope.
/// </summary>
/// <remarks>
/// Lets resources created during a command outlive the command's scope until the end of the whole request
/// (e.g. DbContexts backing an IQueryable returned from a query, which only gets evaluated later).
/// </remarks>
public interface IRequestLifetime
{
    /// <summary>
    /// Registers an object to be disposed at the end of the current request.
    /// </summary>
    /// <returns>False (without registering the object) if there is no current request.</returns>
    bool TryRegisterForDispose(IDisposable disposable);
}
