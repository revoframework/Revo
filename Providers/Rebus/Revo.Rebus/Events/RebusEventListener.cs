using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rebus.Bus;
using Revo.Core.Events;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Rebus.Events
{
    public class RebusEventListener : IAsyncEventListener<IEvent>
    {
        private readonly List<IEventMessage<IEvent>> messages = new List<IEventMessage<IEvent>>();
        private readonly IBus bus;

        public RebusEventListener(RebusEventSequencer rebusEventSequencer, IBus bus)
        {
            EventSequencer = rebusEventSequencer;
            this.bus = bus;
        }

        public Task HandleAsync(IEventMessage<IEvent> message, string sequenceName)
        {
            messages.Add(message);
            return Task.FromResult(0);
        }

        public IAsyncEventSequencer EventSequencer { get; }
        public async Task OnFinishedEventQueueAsync(string sequenceName)
        {
            foreach (IEventMessage<IEvent> message in messages)
            {
                await bus.Publish(message.Event, message.Metadata.ToDictionary(x => x.Key, x => x.Value));
            }
        }
        
        public class RebusEventSequencer : AsyncEventSequencer<IEvent>
        {
            public readonly string QueueNamePrefix = "RebusEventListener:";

            protected override IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<IEvent> message)
            {
                if (!message.Metadata.GetEventId().HasValue)
                {
                    yield break;
                }

                yield return new EventSequencing()
                {
                    SequenceName = QueueNamePrefix,
                    EventSequenceNumber = null
                };
            }

            protected override bool ShouldAttemptSynchronousDispatch(IEventMessage<IEvent> message)
            {
                return false;
            }
        }
    }
}
