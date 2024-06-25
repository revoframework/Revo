using System;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class AsyncEventQueueRecordAdapter(QueuedAsyncEvent queuedEvent, IQueuedAsyncEventMessageFactory messageFactory) : IAsyncEventQueueRecord
    {
        private IEventMessage eventMessage;

        public Guid Id => queuedEvent.Id;
        public Guid EventId => queuedEvent.EventStreamRowId ?? queuedEvent.ExternalEventRecordId
            ?? throw new InvalidOperationException("QueuedAsyncEvent has no event id");
        public string QueueName => queuedEvent.QueueId;
        public long? SequenceNumber => queuedEvent.SequenceNumber;
        public IEventMessage EventMessage => eventMessage ??= messageFactory.CreateEventMessage(queuedEvent);
    }
}
