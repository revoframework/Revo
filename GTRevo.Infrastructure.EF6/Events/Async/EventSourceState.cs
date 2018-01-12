using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.EF6.Events.Async
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
