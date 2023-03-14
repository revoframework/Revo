using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Upgrades;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Upgrades
{
    public class EventStreamSequenceNumbersUpgradeTests
    {
        private EventStreamSequenceNumbersUpgrade sut = new EventStreamSequenceNumbersUpgrade();

        [Fact]
        public void UpgradeSequenceNumbers_AddsAggregateVersionFromSequenceNumber()
        {
            var originalEvent1 = (IEventMessage<IEvent>) EventMessage.FromEvent(new TestEvent1(), new Dictionary<string, string>()
            {
                {BasicEventMetadataNames.StreamSequenceNumber, "1"}
            });

            var events = new[]
            {
                (IEventMessage<DomainAggregateEvent>) UpgradedEventMessage.Create(originalEvent1, new TestEvent1()),
                (IEventMessage<DomainAggregateEvent>) UpgradedEventMessage.Create(originalEvent1, new TestEvent1())
            };

            var result = sut.UpgradeSequenceNumbers(events).ToArray();

            result.Should().HaveCount(2);
            result[0].Event.Should().Be(events[0].Event);
            result[0].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.StreamSequenceNumber, "1"));
            result[0].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.AggregateVersion, "1"));

            result[1].Event.Should().Be(events[1].Event);
            result[1].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.StreamSequenceNumber, "2"));
            result[1].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.AggregateVersion, "1"));
        }

        [Fact]
        public void UpgradeSequenceNumbers_PreservesAggregateVersion()
        {
            var originalEvent1 = (IEventMessage<IEvent>) EventMessage.FromEvent(new TestEvent1(), new Dictionary<string, string>()
            {
                {BasicEventMetadataNames.StreamSequenceNumber, "1"},
                {BasicEventMetadataNames.AggregateVersion, "2"}
            });

            var events = new[]
            {
                (IEventMessage<DomainAggregateEvent>) UpgradedEventMessage.Create(originalEvent1, new TestEvent1()),
                (IEventMessage<DomainAggregateEvent>) UpgradedEventMessage.Create(originalEvent1, new TestEvent1())
            };

            var result = sut.UpgradeSequenceNumbers(events).ToArray();

            result.Should().HaveCount(2);
            result[0].Event.Should().Be(events[0].Event);
            result[0].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.StreamSequenceNumber, "1"));
            result[0].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.AggregateVersion, "2"));

            result[1].Event.Should().Be(events[1].Event);
            result[1].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.StreamSequenceNumber, "2"));
            result[1].Metadata.Should().Contain(new KeyValuePair<string, string>(BasicEventMetadataNames.AggregateVersion, "2"));
        }

        public class TestEvent1 : DomainAggregateEvent
        {
        }
    }
}