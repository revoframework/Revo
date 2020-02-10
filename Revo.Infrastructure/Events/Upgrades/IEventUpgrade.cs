using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public interface IEventUpgrade
    {
        IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeStream(IEnumerable<IEventMessage<DomainAggregateEvent>> events,
            IReadOnlyDictionary<string, string> eventStreamMetadata);
    }
}