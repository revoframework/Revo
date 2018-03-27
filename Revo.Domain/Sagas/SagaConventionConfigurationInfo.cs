using System;
using System.Collections.Generic;

namespace Revo.Domain.Sagas
{
    public class SagaConfigurationInfo
    {
        public SagaConfigurationInfo(IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> events)
        {
            Events = events;
        }

        public IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> Events { get; }
    }
}
