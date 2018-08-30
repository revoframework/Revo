using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Events.Async.Generic
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "ESS")]
    [DatabaseEntity]
    public class EventSourceState : IRowVersioned
    {
        public EventSourceState(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
        public string EventEnqueueCheckpoint { get; set; }
        public int Version { get; set; }
    }
}
