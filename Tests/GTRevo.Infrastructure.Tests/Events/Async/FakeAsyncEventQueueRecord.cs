using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Events.Async;

namespace GTRevo.Infrastructure.Tests.Events.Async
{
    public class FakeAsyncEventQueueRecord : IAsyncEventQueueRecord
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public string QueueName { get; set; }
        public long? SequenceNumber { get; set; }
        public IEventMessage EventMessage { get; set; }
    }
}
