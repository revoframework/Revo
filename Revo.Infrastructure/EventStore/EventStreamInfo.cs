using System;

namespace Revo.Infrastructure.EventStore
{
    public class EventStreamInfo
    {
        public EventStreamInfo(Guid id, long eventCount, long version)
        {
            Id = id;
            EventCount = eventCount;
            Version = version;
        }

        public Guid Id { get; }
        public long EventCount { get; }
        public long Version { get; }
        //public bool IsArchived { get; }
    }
}
