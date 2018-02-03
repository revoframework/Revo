using System;
using System.Collections.ObjectModel;

namespace Revo.Domain.Sagas
{
    public interface ISagaConventionConfigurationCache
    {
        ReadOnlyDictionary<Type, SagaConfigurationInfo> ConfigurationInfos { get; }
    }
}