using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Core.Domain
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
