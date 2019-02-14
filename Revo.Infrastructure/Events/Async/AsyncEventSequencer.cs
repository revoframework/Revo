using System.Collections.Generic;
using System.Linq;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public abstract class AsyncEventSequencer<TEvent> : IAsyncEventSequencer<TEvent>
        where TEvent : IEvent
    {
        public IEnumerable<EventSequencing> GetEventSequencing(IEventMessage message)
        {
            if (message is IEventMessage<TEvent> castedMessage)
            {
                return GetEventSequencing(castedMessage);
            }

            return Enumerable.Empty<EventSequencing>();
        }

        /*public virtual IAsyncEventQueueGovernor GetQueueGovernor(IReadOnlyCollection<IAsyncEventQueueRecord> events)
        {
            return null;
        }*/

        public bool ShouldAttemptSynchronousDispatch(IEventMessage message)
        {
            if (message is IEventMessage<TEvent> castedMessage)
            {
                return ShouldAttemptSynchronousDispatch(castedMessage);
            }

            return false;
        }

        protected abstract IEnumerable<EventSequencing> GetEventSequencing(IEventMessage<TEvent> message);
        protected abstract bool ShouldAttemptSynchronousDispatch(IEventMessage<TEvent> message);
    }
}
