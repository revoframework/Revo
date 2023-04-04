using System.Collections.Generic;
using System.Reflection;
using Revo.Core.Types;

namespace Revo.Core.Configuration;

public class CoreConfigurationSection : IRevoConfigurationSection, ITypesConfiguration
{
    private readonly HashSet<Assembly> assemblies = new();

    public bool IsAppDomainTypeDiscoveryEnabled { get; private set; }

    /// <summary>
    /// Can override if the current environment is treated as a development environment (contrary to production environment).
    /// This may affect logging, database migration, etc.
    /// </summary>
    public bool? IsDevelopmentEnvironment { get; set; }

    public IReadOnlyCollection<Assembly> Assemblies => assemblies;

    public CoreConfigurationSection AddAssembly(params Assembly[] addedAssemblies)
    {
        foreach (var assembly in addedAssemblies)
        {
            assemblies.Add(assembly);
        }
            
        return this;
    }
    
    public CoreConfigurationSection UseAllAppDomainAssembliesForTypeDiscovery(bool useAll = true)
    {
        IsAppDomainTypeDiscoveryEnabled = useAll;
        return this;
    }
}