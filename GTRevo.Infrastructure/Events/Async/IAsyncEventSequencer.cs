using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventSequencer
    {
        IEnumerable<EventSequencing> GetEventSequencing(IEventMessage message);
        bool ShouldAttemptSynchronousDispatch(IEventMessage message);
    }

    public interface IAsyncEventSequencer<in TEvent> : IAsyncEventSequencer
        where TEvent : IEvent
    {
    }
}
