using System;
using System.Collections.Generic;

namespace Revo.Domain.Sagas
{
    public class SagaConfigurationInfo(IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> events)
    {
        public IReadOnlyDictionary<Type, IReadOnlyCollection<SagaConventionEventInfo>> Events { get; } = events;
    }
}
