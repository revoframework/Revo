using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Services;

/// <summary>
/// Index of the generic service types registered in a service collection, used by
/// <see cref="VariantServiceResolver"/> to find the registrations compatible with a requested variant type.
/// </summary>
/// <remarks>
/// Keeps a reference to the live <see cref="IServiceCollection"/> and indexes it lazily upon the first query,
/// so that the registrations added after this index was created (e.g. by the application after AddRevo)
/// are still included. The service collection must not be modified after the first query.
/// </remarks>
public sealed class VariantServiceTypeIndex
{
    private readonly Lazy<Registrations> registrations;
    private readonly ConcurrentDictionary<Type, Type[]> variantServiceTypesCache = new();
    private readonly ConcurrentDictionary<Type, Type[]> openGenericImplementationTypesCache = new();

    public VariantServiceTypeIndex(IServiceCollection services)
    {
        registrations = new Lazy<Registrations>(() => new Registrations(services),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    /// <summary>
    /// Returns the service types (other than <paramref name="serviceType"/> itself) that have closed registrations
    /// and whose instances are assignable to <paramref name="serviceType"/> through generic variance,
    /// in the order of their registration.
    /// </summary>
    public IReadOnlyList<Type> GetVariantServiceTypes(Type serviceType)
    {
        if (!IsVariantGenericType(serviceType))
        {
            return [];
        }

        return variantServiceTypesCache.GetOrAdd(serviceType, FindVariantServiceTypes);
    }

    /// <summary>
    /// Returns the implementation types that the container constructs for <paramref name="serviceType"/>
    /// from its open generic registrations (one per registration).
    /// </summary>
    public IReadOnlyList<Type> GetOpenGenericImplementationTypes(Type serviceType)
    {
        if (!serviceType.IsConstructedGenericType)
        {
            return [];
        }

        return openGenericImplementationTypesCache.GetOrAdd(serviceType, FindOpenGenericImplementationTypes);
    }

    private Type[] FindVariantServiceTypes(Type serviceType)
    {
        if (!registrations.Value.ClosedServiceTypesByDefinition.TryGetValue(
                serviceType.GetGenericTypeDefinition(), out var candidateTypes))
        {
            return [];
        }

        return candidateTypes
            .Where(x => x != serviceType && serviceType.IsAssignableFrom(x))
            .ToArray();
    }

    private Type[] FindOpenGenericImplementationTypes(Type serviceType)
    {
        if (!registrations.Value.OpenGenericImplementationTypesByDefinition.TryGetValue(
                serviceType.GetGenericTypeDefinition(), out var openImplementationTypes))
        {
            return [];
        }

        var implementationTypes = new List<Type>();
        foreach (var openImplementationType in openImplementationTypes)
        {
            try
            {
                implementationTypes.Add(openImplementationType.MakeGenericType(serviceType.GenericTypeArguments));
            }
            catch (ArgumentException)
            {
                // generic constraints not satisfied, so the container does not construct this one either
            }
        }

        return implementationTypes.ToArray();
    }

    private static bool IsVariantGenericType(Type type)
    {
        return type.IsConstructedGenericType
               && type.GetGenericTypeDefinition().GetGenericArguments().Any(x =>
                   (x.GenericParameterAttributes & GenericParameterAttributes.VarianceMask) != 0);
    }

    private sealed class Registrations
    {
        public Registrations(IServiceCollection services)
        {
            // keyed services are not returned by non-keyed resolution, so they must not be returned here either
            var nonKeyedDescriptors = services.Where(x => !x.IsKeyedService).ToArray();

            ClosedServiceTypesByDefinition = nonKeyedDescriptors
                .Where(x => x.ServiceType.IsConstructedGenericType)
                .Select(x => x.ServiceType)
                .Distinct()
                .GroupBy(x => x.GetGenericTypeDefinition())
                .ToDictionary(x => x.Key, x => x.ToArray());

            OpenGenericImplementationTypesByDefinition = nonKeyedDescriptors
                .Where(x => x.ServiceType.IsGenericTypeDefinition && x.ImplementationType != null)
                .GroupBy(x => x.ServiceType)
                .ToDictionary(x => x.Key, x => x.Select(d => d.ImplementationType).ToArray());
        }

        public IReadOnlyDictionary<Type, Type[]> ClosedServiceTypesByDefinition { get; }
        public IReadOnlyDictionary<Type, Type[]> OpenGenericImplementationTypesByDefinition { get; }
    }
}
