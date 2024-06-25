using System;

namespace Revo.Infrastructure.EventStores
{
    public class EventStreamInfo(Guid id, long eventCount, long version)
    {


        public Guid Id { get; } = id;
        public long EventCount { get; } = eventCount;
        public long Version { get; } = version;
        //public bool IsArchived { get; }
    }
}
