using System;
using System.Collections.ObjectModel;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface ISagaConventionConfigurationCache
    {
        ReadOnlyDictionary<Type, SagaConfigurationInfo> ConfigurationInfos { get; }
    }
}