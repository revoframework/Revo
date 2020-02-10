using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public interface IEventStreamUpgrader
    {
        IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeStream(
            IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
            IReadOnlyDictionary<string, string> eventStreamMetadata);
    }
}