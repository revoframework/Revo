using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Revo.Core.Types;

namespace Revo.Infrastructure.Projections;

public class ProjectorDiscovery
{
    private readonly Type[] genericProjectorInterfaces;
    private readonly ITypeExplorer typeExplorer;
    private readonly ILogger logger;

    public ProjectorDiscovery(ITypeExplorer typeExplorer, Type[] genericProjectorInterfaces, ILogger logger)
    {
        this.typeExplorer = typeExplorer;
        this.genericProjectorInterfaces = genericProjectorInterfaces;
        this.logger = logger;
    }

    public void DiscoverProjectors(IServiceCollection serviceCollection)
    {
        var projectorTypes = typeExplorer.GetAllTypes()
            .Where(x => x.IsClass && !x.IsAbstract && !x.IsGenericTypeDefinition
                && GetProjectorInterfaces(x).Length > 0)
            .ToArray();

        RegisterProjectors(serviceCollection, projectorTypes);
        logger.LogDebug($"Discovered {projectorTypes.Length} projectors: {string.Join(", ", projectorTypes.Select(x => x.FullName))}");
    }

    protected virtual Type[] GetGenericProjectorInterfaces() => genericProjectorInterfaces;

    private Type[] GetProjectorInterfaces(Type requestHandlerType)
    {
        var intfs = GetInterfaces(requestHandlerType);

        intfs = intfs
            .Where(x => x.IsGenericType
                        && GetGenericProjectorInterfaces()
                            .Contains(x.GetGenericTypeDefinition()));

        return intfs.Distinct().ToArray();
    }

    private static IEnumerable<Type> GetInterfaces(Type handlerType)
    {
        var intfs = (IEnumerable<Type>)handlerType.GetInterfaces();
        var nestedIntfs = intfs.SelectMany(x => x.GetInterfaces().Length > 0 ? GetInterfaces(x) : Array.Empty<Type>()).ToArray();
        if (nestedIntfs.Length > 0)
        {
            intfs = intfs.Concat(nestedIntfs);
        }

        return intfs;
    }

    private void RegisterProjectors(IServiceCollection serviceCollection, IEnumerable<Type> projectorTypes)
    {
        foreach (Type projectorType in projectorTypes)
        {
            foreach (var projectorInterface in GetProjectorInterfaces(projectorType))
            {
                serviceCollection.AddScoped(projectorInterface, projectorType);
            }
        }
    }
}
