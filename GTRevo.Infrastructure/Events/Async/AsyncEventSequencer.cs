using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
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
