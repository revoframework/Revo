using System;
using System.Collections.Generic;

namespace Revo.Domain.Sagas
{
    public class SagaConfigurationInfo
    {
        public SagaConfigurationInfo(IReadOnlyDictionary<Type, SagaConventionEventInfo> events)
        {
            Events = events;
        }

        public IReadOnlyDictionary<Type, SagaConventionEventInfo> Events { get; }
    }
}
