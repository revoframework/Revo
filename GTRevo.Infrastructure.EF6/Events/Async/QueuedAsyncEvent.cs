using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore.Model;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.EF6.Events.Async
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "QAE")]
    [DatabaseEntity]
    public class QueuedAsyncEvent : IAsyncEventQueueRecord
    {
        private IEventMessage @event;

        public QueuedAsyncEvent(string queueId, EventStreamRow eventStreamRow, long? sequenceNumber)
        {
            QueueId = queueId;
            EventId = eventStreamRow.Id;
            EventStreamRow = eventStreamRow;
            SequenceNumber = sequenceNumber;
            Id = Guid.NewGuid();
        }

        public QueuedAsyncEvent(string queueId, ExternalEventRecord externalEventRecord, long? sequenceNumber)
        {
            QueueId = queueId;
            EventId = ExternalEventRecord.Id;
            ExternalEventRecord = externalEventRecord;
            SequenceNumber = sequenceNumber;
            Id = Guid.NewGuid();
        }

        protected QueuedAsyncEvent()
        {
        }

        public Guid Id { get; private set; }
        public Guid EventId { get; private set; }
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
                        @event = GTRevo.Core.Events.EventMessage.FromEvent(ExternalEventRecord.Event, ExternalEventRecord.Metadata);
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
