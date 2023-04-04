using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Revo.Core.Types;

public class TypeExplorer(ITypesConfiguration configuration, ILogger<TypeExplorer> logger) : ITypeExplorer
{
    private readonly HashSet<Assembly> assemblies = configuration.Assemblies.ToHashSet();

    public virtual IEnumerable<Assembly> GetAllAssemblies()
    {
        return configuration.IsAppDomainTypeDiscoveryEnabled
            ? AppDomain.CurrentDomain.GetAssemblies()
            : assemblies;
    }

    public IEnumerable<Type> GetAllTypes()
    {
        var assemblies = GetAllAssemblies();
        return assemblies.SelectMany(x =>
        {
            try
            {
                return x.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                logger.LogWarning(e, $"Could not load types from assembly {x}, loader exceptions: {string.Join("\n", e.LoaderExceptions.Select(x => x.ToString()))}");
                return new Type[0];
            }
        });
    }

    public Type FindType(string typeName)
    {
        foreach (var type in GetAllTypes())
        {
            if (type.FullName == typeName)
                return type;
        }
        return null;
    }
}