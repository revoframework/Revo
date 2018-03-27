using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Revo.Domain.Sagas
{
    public interface ISagaConventionConfigurationCache
    {
        IReadOnlyDictionary<Type, SagaConfigurationInfo> ConfigurationInfos { get; }
    }
}