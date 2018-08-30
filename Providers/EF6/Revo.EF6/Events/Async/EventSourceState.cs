using Revo.DataAccess.Entities;

namespace Revo.EF6.Events.Async
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
