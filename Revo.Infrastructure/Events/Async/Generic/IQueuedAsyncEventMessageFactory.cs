using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public interface IQueuedAsyncEventMessageFactory
    {
        IEventMessage CreateEventMessage(QueuedAsyncEvent queuedEvent);
    }
}