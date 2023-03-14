using System;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Tests.Events.Async
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
