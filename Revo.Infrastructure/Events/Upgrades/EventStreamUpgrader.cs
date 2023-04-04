using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades;

public class EventStreamUpgrader : IEventStreamUpgrader
{
    private readonly IServiceProvider serviceProvider;
    private readonly IEventStreamSequenceNumbersUpgrade sequenceNumbersUpgrade;

    public EventStreamUpgrader(IServiceProvider serviceProvider,
        IEventStreamSequenceNumbersUpgrade sequenceNumbersUpgrade)
    {
        this.serviceProvider = serviceProvider;
        this.sequenceNumbersUpgrade = sequenceNumbersUpgrade;
    }

    public IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeStream(
        IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
        IReadOnlyDictionary<string, string> eventStreamMetadata)
    {
        var eventUpgrades = serviceProvider.GetServices<IEventUpgrade>();

        var result = eventStream;
        foreach (var eventUpgrade in eventUpgrades)
        {
            result = eventUpgrade.UpgradeStream(result, eventStreamMetadata);
        }

        result = sequenceNumbersUpgrade.UpgradeSequenceNumbers(result);

        return result;
    }
}
