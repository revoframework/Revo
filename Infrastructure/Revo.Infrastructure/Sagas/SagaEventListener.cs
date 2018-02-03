using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Sagas
{
    public class SagaEventListener :
        IAsyncEventListener<DomainEvent>
    {
        private readonly ISagaLocator sagaLocator;
        private readonly List<IEventMessage<DomainEvent>> bufferedEvents = new List<IEventMessage<DomainEvent>>();

        public SagaEventListener(ISagaLocator sagaLocator, SagaEventSequencer sagaEventSequencer)
        {
            this.sagaLocator = sagaLocator;
            EventSequencer = sagaEventSequencer;
        }

        public Task HandleAsync(IEventMessage<DomainEvent> message, string sequenceName)
        {
            bufferedEvents.Add(message);
            return Task.FromResult(0);
        }

        public IAsyncEventSequencer EventSequencer { get; }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            return DispatchSagaEvents();
        }

        private async Task DispatchSagaEvents()
        {
            await sagaLocator.LocateAndDispatchAsync(bufferedEvents);
            bufferedEvents.Clear();
        }

        public class SagaEventSequencer : AsyncEventSequencer<DomainEvent>
        {
            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<DomainEvent> message)
            {
                yield return new EventSequencing()
                {
                    SequenceName = message.Event is DomainAggregateEvent domainAggregateEvent
                        ? "SagaEventListener:" + domainAggregateEvent.AggregateId.ToString() // TODO queue per saga instance
                        : "SagaEventListener",
                    EventSequenceNumber = message.Metadata.GetStreamSequenceNumber()
                };
            }

            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<DomainEvent> message)
            {
                return false;
            }
        }
    }
}
