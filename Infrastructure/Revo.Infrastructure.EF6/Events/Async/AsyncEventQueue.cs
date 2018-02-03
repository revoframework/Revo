using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.EF6.Events.Async
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
