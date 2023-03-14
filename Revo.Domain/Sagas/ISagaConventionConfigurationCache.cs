using System;
using System.Collections.Generic;

namespace Revo.Domain.Sagas
{
    public interface ISagaConventionConfigurationCache
    {
        IReadOnlyDictionary<Type, SagaConfigurationInfo> ConfigurationInfos { get; }
    }
}