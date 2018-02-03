using System;
using System.ComponentModel.DataAnnotations.Schema;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.Events.Async
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "QAE")]
    [DatabaseEntity]
    public class QueuedAsyncEvent : IAsyncEventQueueRecord
    {
        private IEventMessage @event;

        public QueuedAsyncEvent(string queueId, EventStreamRow eventStreamRow, long? sequenceNumber)
        {
            QueueId = queueId;
            EventStreamRow = eventStreamRow;
            EventStreamRowId = eventStreamRow.Id;
            SequenceNumber = sequenceNumber;
            Id = Guid.NewGuid();
        }

        public QueuedAsyncEvent(string queueId, ExternalEventRecord externalEventRecord, long? sequenceNumber)
        {
            QueueId = queueId;
            ExternalEventRecord = externalEventRecord;
            ExternalEventRecordId = externalEventRecord.Id;
            SequenceNumber = sequenceNumber;
            Id = Guid.NewGuid();
        }

        protected QueuedAsyncEvent()
        {
        }

        public Guid Id { get; private set; }
        public Guid EventId => EventStreamRowId ?? ExternalEventRecordId ?? throw new InvalidOperationException("QueuedAsyncEvent has no event id");
        public long? SequenceNumber { get; private set; }

        [NotMapped]
        public IEventMessage EventMessage
        {
            get
            {
                if (@event == null)
                {
                    if (EventStreamRow != null)
                    {
                        @event = EventStoreEventMessage.FromRecord(EventStreamRow);
                    }
                    else
                    {
                        @event = Revo.Core.Events.EventMessage.FromEvent(ExternalEventRecord.Event, ExternalEventRecord.Metadata);
                    }
                }

                return @event;
            }
        }

        public AsyncEventQueue Queue { get; private set; }
        public string QueueId { get; private set; }
        public string QueueName => QueueId;
        public EventStreamRow EventStreamRow { get; private set; }
        public Guid? EventStreamRowId { get; private set; }
        public ExternalEventRecord ExternalEventRecord { get; private set; }
        public Guid? ExternalEventRecordId { get; private set; }
    }
}
