using Revo.DataAccess.Entities;

namespace Revo.Infrastructure.Events.Async.Generic
{
    [TablePrefix(NamespacePrefix = "RAE", ColumnPrefix = "ESS")]
    [DatabaseEntity]
    public class EventSourceState(string id) : IRowVersioned
    {
        public string Id { get; private set; } = id;
        public string EventEnqueueCheckpoint { get; set; }
        public int Version { get; set; }
    }
}
