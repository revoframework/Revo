using System;
using System.Collections.Generic;

namespace Revo.Core.Services;

/// <summary>
/// Resolves services while honoring generic variance of the requested service type, which the stock
/// Microsoft.Extensions.DependencyInjection container does not do.
/// </summary>
/// <remarks>
/// E.g. requesting <c>IEventListener&lt;MyEvent&gt;</c> also returns services registered as
/// <c>IEventListener&lt;IEvent&gt;</c>, because <c>IEventListener&lt;in T&gt;</c> is contravariant.
/// Services registered for the exact requested type always come first (including open generic registrations),
/// followed by the variance-compatible registrations in the order of their registration.
/// <para>Only constructed generic service types whose generic type definition declares variant type parameters
/// are affected; any other service type is resolved exactly as by the underlying <see cref="IServiceProvider"/>.</para>
/// </remarks>
public interface IVariantServiceResolver
{
    /// <summary>
    /// Returns all services assignable to <paramref name="serviceType"/>, i.e. services registered for the exact type
    /// followed by those registered for its variance-compatible types.
    /// </summary>
    IEnumerable<object> GetServices(Type serviceType);

    /// <summary>
    /// Returns the service registered for the exact <paramref name="serviceType"/> or, if there is none,
    /// the service registered for its only variance-compatible type. Returns null when no such service is registered.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is no exact registration and there are multiple
    /// variance-compatible registered service types to choose from.</exception>
    object GetService(Type serviceType);
}
