using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public class EventStreamSequenceNumbersUpgrade : IEventStreamSequenceNumbersUpgrade
    {
        public IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeSequenceNumbers(
            IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream)
        {
            int sequenceNumber = 1;
            foreach (var message in eventStream)
            {
                long messageNumber = sequenceNumber;
                long? originalSequenceNumber = message.Metadata.GetStreamSequenceNumber();

                UpgradedEventMessage upgradedMessage = message as UpgradedEventMessage;

                if (originalSequenceNumber != messageNumber)
                {
                    upgradedMessage ??= UpgradedEventMessage.Create(message, message.Event);
                    upgradedMessage.MetadataOverrides[BasicEventMetadataNames.StreamSequenceNumber] = () => messageNumber.ToString();
                }

                if (upgradedMessage != null)
                {
                    if (!upgradedMessage.Metadata.ContainsKey(BasicEventMetadataNames.AggregateVersion)
                        && originalSequenceNumber != null)
                    {
                        upgradedMessage.MetadataOverrides[BasicEventMetadataNames.AggregateVersion] = () => originalSequenceNumber.ToString();
                    }

                    yield return (IEventMessage<DomainAggregateEvent>) upgradedMessage;
                }
                else
                {
                    yield return message;
                }

                sequenceNumber++;
            }
        }
    }
}