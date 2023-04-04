using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Upgrades;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Upgrades
{
    public class EventStreamUpgraderTests
    {
        private IEventStreamSequenceNumbersUpgrade sequenceNumbersUpgrade;
        private EventStreamUpgrader sut;

        public EventStreamUpgraderTests()
        {
            sequenceNumbersUpgrade = Substitute.For<IEventStreamSequenceNumbersUpgrade>();
        }

        [Fact]
        public void UpgradeStream_RunsAllUpgrades()
        {
            var eventStream = new[] {(IEventMessage<DomainAggregateEvent>)EventMessage.FromEvent(Substitute.ForPartsOf<DomainAggregateEvent>(), null)};
            var eventStream1 = new[] {(IEventMessage<DomainAggregateEvent>)EventMessage.FromEvent(Substitute.ForPartsOf<DomainAggregateEvent>(), null)};
            var eventStream2 = new[] {(IEventMessage<DomainAggregateEvent>)EventMessage.FromEvent(Substitute.ForPartsOf<DomainAggregateEvent>(), null)};
            var eventStream3 = new[] {(IEventMessage<DomainAggregateEvent>)EventMessage.FromEvent(Substitute.ForPartsOf<DomainAggregateEvent>(), null)};
            var streamMetadata = new Dictionary<string, string>();

            var eventUpgrades = new[]
            {
                Substitute.For<IEventUpgrade>(),
                Substitute.For<IEventUpgrade>()
            };

            eventUpgrades[0].UpgradeStream(eventStream, streamMetadata).Returns(eventStream1);
            eventUpgrades[1].UpgradeStream(eventStream1, streamMetadata).Returns(eventStream2);

            var serviceProvider = Substitute.For<IServiceProvider>();
            serviceProvider.GetService(typeof(IEnumerable<IEventUpgrade>))
                .Returns(eventUpgrades);

            sequenceNumbersUpgrade.UpgradeSequenceNumbers(eventStream2).Returns(eventStream3);

            sut = new EventStreamUpgrader(serviceProvider, sequenceNumbersUpgrade);
            var result = sut.UpgradeStream(eventStream, streamMetadata);

            result.Should().BeSameAs(eventStream3);
        }
    }
}