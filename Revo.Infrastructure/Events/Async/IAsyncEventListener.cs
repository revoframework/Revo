using System.Threading.Tasks;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
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
