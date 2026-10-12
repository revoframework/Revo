using System;
using System.Collections.Generic;
using System.Linq;

namespace Revo.Core.Services;

public static class VariantServiceResolverExtensions
{
    public static IEnumerable<T> GetServices<T>(this IVariantServiceResolver serviceResolver)
    {
        return serviceResolver.GetServices(typeof(T)).Cast<T>().ToArray();
    }

    public static T GetService<T>(this IVariantServiceResolver serviceResolver)
    {
        return (T) serviceResolver.GetService(typeof(T));
    }

    public static object GetRequiredService(this IVariantServiceResolver serviceResolver, Type serviceType)
    {
        return serviceResolver.GetService(serviceType)
               ?? throw new InvalidOperationException(
                   $"No service for type {serviceType} (nor for any of its variance-compatible types) has been registered.");
    }

    public static T GetRequiredService<T>(this IVariantServiceResolver serviceResolver)
    {
        return (T) serviceResolver.GetRequiredService(typeof(T));
    }
}
