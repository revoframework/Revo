using System.Collections.Generic;
using System.Linq;
using Revo.Core.Configuration;

namespace Revo.Core.Core;

public class Environment : IEnvironment
{
    private readonly IEnumerable<IEnvironmentProvider> environmentProviders;
    private readonly CoreConfigurationSection coreConfigurationSection;

    public Environment(IEnumerable<IEnvironmentProvider> environmentProviders,
        CoreConfigurationSection coreConfigurationSection)
    {
        this.environmentProviders = environmentProviders;
        this.coreConfigurationSection = coreConfigurationSection;

        IsDevelopment = IsDevelopmentOverride
                        ?? environmentProviders.Any(x => x.IsDevelopment == true);
    }

    public bool IsDevelopment { get; private set; }
    public bool? IsDevelopmentOverride => coreConfigurationSection.IsDevelopmentEnvironment;
}