using System;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class AsyncEventQueueRecordAdapter : IAsyncEventQueueRecord
    {
        private readonly QueuedAsyncEvent queuedEvent;
        private readonly IQueuedAsyncEventMessageFactory messageFactory;
        private IEventMessage eventMessage;

        public AsyncEventQueueRecordAdapter(QueuedAsyncEvent queuedEvent, IQueuedAsyncEventMessageFactory messageFactory)
        {
            this.queuedEvent = queuedEvent;
            this.messageFactory = messageFactory;
        }

        public Guid Id => queuedEvent.Id;
        public Guid EventId => queuedEvent.EventStreamRowId ?? queuedEvent.ExternalEventRecordId
            ?? throw new InvalidOperationException("QueuedAsyncEvent has no event id");
        public string QueueName => queuedEvent.QueueId;
        public long? SequenceNumber => queuedEvent.SequenceNumber;
        public IEventMessage EventMessage => eventMessage ??= messageFactory.CreateEventMessage(queuedEvent);
    }
}
