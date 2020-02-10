using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public class UpgradedEventStream : List<IEventMessage<DomainAggregateEvent>>
    {
        public UpgradedEventStream()
        {
        }

        public UpgradedEventStream(IEnumerable<IEventMessage<DomainAggregateEvent>> collection) : base(collection)
        {
        }

        public UpgradedEventStream(int capacity) : base(capacity)
        {
        }

        public bool IsUpgraded { get; private set; }

        public void FlagUpdated()
        {
            IsUpgraded = true;
        }
    }
}