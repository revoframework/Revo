using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
{
    public interface IAsyncEventListener
    {
        IAsyncEventSequencer EventSequencer { get; }
        Task OnFinishedEventQueueAsync(string sequenceName);
    }

    public interface IAsyncEventListener<in TEvent> : IAsyncEventListener
        where TEvent : IEvent
    {
        Task HandleAsync(IEventMessage<TEvent> message, string sequenceName);
    }
}
