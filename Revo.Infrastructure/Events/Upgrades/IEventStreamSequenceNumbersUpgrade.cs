using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public interface IEventStreamSequenceNumbersUpgrade
    {
        IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeSequenceNumbers(
            IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream);
    }
}