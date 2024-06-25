using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Sagas
{
    public class SagaEventSequencer : AsyncEventSequencer<DomainEvent>
    {
        protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<DomainEvent> message)
        {
            yield return new EventSequencing()
            {
                SequenceName = message.Event is DomainAggregateEvent domainAggregateEvent
                    ? "SagaEventListener:" + domainAggregateEvent.AggregateId.ToString()
                    : "SagaEventListener",
                EventSequenceNumber = message.Event is DomainAggregateEvent
                    ? message.Metadata.GetStreamSequenceNumber() : null
            };
        }

        protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<DomainEvent> message)
        {
            return false;
        }
    }
}
