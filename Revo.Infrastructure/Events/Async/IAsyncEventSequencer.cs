using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventSequencer
    {
        IEnumerable<EventSequencing> GetEventSequencing(IEventMessage message);
        //IAsyncEventQueueGovernor GetQueueGovernor(IReadOnlyCollection<IAsyncEventQueueRecord> events);
        bool ShouldAttemptSynchronousDispatch(IEventMessage message);
    }

    public interface IAsyncEventSequencer<in TEvent> : IAsyncEventSequencer
        where TEvent : IEvent
    {
    }
}
