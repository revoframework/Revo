using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
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
