using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public class EventStreamUpgrader : IEventStreamUpgrader
    {
        private readonly Func<IEventUpgrade[]> eventUpgradesFunc;
        private readonly IEventStreamSequenceNumbersUpgrade sequenceNumbersUpgrade;

        public EventStreamUpgrader(Func<IEventUpgrade[]> eventUpgradesFunc,
            IEventStreamSequenceNumbersUpgrade sequenceNumbersUpgrade)
        {
            this.eventUpgradesFunc = eventUpgradesFunc;
            this.sequenceNumbersUpgrade = sequenceNumbersUpgrade;
        }

        public IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeStream(
            IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
            IReadOnlyDictionary<string, string> eventStreamMetadata)
        {
            var eventUpgrades = eventUpgradesFunc();

            var result = eventStream;
            foreach (var eventUpgrade in eventUpgrades)
            {
                result = eventUpgrade.UpgradeStream(result, eventStreamMetadata);
            }

            result = sequenceNumbersUpgrade.UpgradeSequenceNumbers(result);

            return result;
        }
    }
}
