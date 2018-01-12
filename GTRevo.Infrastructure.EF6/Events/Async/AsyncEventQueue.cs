using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Events.Async;

namespace GTRevo.Infrastructure.EF6.Events.Async
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "AEQ")]
    [DatabaseEntity]
    public class AsyncEventQueue : IAsyncEventQueueState, IRowVersioned
    {
        public AsyncEventQueue(string id, long? lastSequenceNumberProcessed)
        {
            Id = id;
            LastSequenceNumberProcessed = lastSequenceNumberProcessed;
        }

        protected AsyncEventQueue()
        {
        }

        public string Id { get; private set; }
        public int Version { get; set; }
        public long? LastSequenceNumberProcessed { get; set; }
    }
}
