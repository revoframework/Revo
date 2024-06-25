using System;
using Revo.Core.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class QueuedAsyncEventMessageFactory(IEventSerializer eventSerializer) : IQueuedAsyncEventMessageFactory
    {
        public IEventMessage CreateEventMessage(QueuedAsyncEvent queuedEvent)
        {
            if (queuedEvent.EventStreamRow != null)
            {
                var record = new EventStoreRecordAdapter(queuedEvent.EventStreamRow, eventSerializer);
                return EventStoreEventMessage.FromRecord(record);
            }
            else if (queuedEvent.ExternalEventRecord != null)
            {
                return ExternalEventMessageAdapter.FromRecord(queuedEvent.ExternalEventRecord, eventSerializer);
            }

            throw new ArgumentException(
                $"Cannot create event message for QueuedAsyncEvent with ID {queuedEvent.Id} because both EventStreamRow and ExternalEventRecord are null");
        }
    }
}