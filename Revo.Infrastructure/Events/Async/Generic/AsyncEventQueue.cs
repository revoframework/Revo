using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Events.Async.Generic
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "AEQ")]
    [DatabaseEntity]
    public class AsyncEventQueue : IHasId<string>, IAsyncEventQueueState, IRowVersioned
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
