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
        private readonly ISagaEventDispatcher sagaEventDispatcher;
        private readonly List<IEventMessage<DomainEvent>> bufferedEvents = new List<IEventMessage<DomainEvent>>();

        public SagaEventListener(ISagaEventDispatcher sagaEventDispatcher, SagaEventSequencer sagaEventSequencer)
        {
            this.sagaEventDispatcher = sagaEventDispatcher;
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
            await sagaEventDispatcher.DispatchEventsToSagas(bufferedEvents);
            bufferedEvents.Clear();
        }

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
}
