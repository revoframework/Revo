using System;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.Infrastructure.Events.Async.Generic
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "QAE")]
    [DatabaseEntity]
    public class QueuedAsyncEvent
    {
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
        
        public long? SequenceNumber { get; private set; }
        
        public AsyncEventQueue Queue { get; private set; }
        public string QueueId { get; private set; }
        public EventStreamRow EventStreamRow { get; private set; }
        public Guid? EventStreamRowId { get; private set; }
        public ExternalEventRecord ExternalEventRecord { get; private set; }
        public Guid? ExternalEventRecordId { get; private set; }
    }
}
