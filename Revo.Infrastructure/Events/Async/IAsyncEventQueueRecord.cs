using System;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueRecord
    {
        Guid Id { get; }
        Guid EventId { get; }
        string QueueName { get; }
        long? SequenceNumber { get; }
        IEventMessage EventMessage { get; }
    }
}
