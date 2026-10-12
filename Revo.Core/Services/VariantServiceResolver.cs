using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Services;

public class VariantServiceResolver(
    IServiceProvider serviceProvider,
    VariantServiceTypeIndex serviceTypeIndex)
    : IVariantServiceResolver
{
    public IEnumerable<object> GetServices(Type serviceType)
    {
        var services = serviceProvider.GetServices(serviceType).ToList();
        foreach (var variantServiceType in serviceTypeIndex.GetVariantServiceTypes(serviceType))
        {
            services.AddRange(GetClosedRegistrationServices(variantServiceType));
        }

        return services;
    }

    public object GetService(Type serviceType)
    {
        var service = serviceProvider.GetService(serviceType);
        if (service != null)
        {
            return service;
        }

        var variantServiceTypes = serviceTypeIndex.GetVariantServiceTypes(serviceType);
        if (variantServiceTypes.Count > 1)
        {
            throw new InvalidOperationException(
                $"Cannot resolve a single service of type {serviceType} because there are multiple variance-compatible service types registered: "
                + string.Join(", ", variantServiceTypes));
        }

        return variantServiceTypes.Count == 1
            ? GetClosedRegistrationServices(variantServiceTypes[0]).LastOrDefault()
            : null;
    }

    /// <summary>
    /// Resolves the services registered for a variant service type, leaving out the ones the container constructs
    /// from open generic registrations - those have already been resolved for the exact requested type
    /// and would otherwise be returned twice (e.g. FilterCommandBusMiddleware&lt;&gt; would run for both
    /// ICommandBusMiddleware&lt;MyCommand&gt; and ICommandBusMiddleware&lt;ICommandBase&gt;).
    /// </summary>
    private List<object> GetClosedRegistrationServices(Type variantServiceType)
    {
        var services = serviceProvider.GetServices(variantServiceType).ToList();
        foreach (var implementationType in serviceTypeIndex.GetOpenGenericImplementationTypes(variantServiceType))
        {
            int index = services.FindIndex(x => x?.GetType() == implementationType);
            if (index >= 0)
            {
                services.RemoveAt(index);
            }
        }

        return services;
    }
}
