using System.Collections.Generic;

namespace GTRevo.Infrastructure.EventStore
{
    public class EventStreamSlice
    {
        public EventStreamSlice(IEnumerable<IEventStoreRecord> events, IStreamPosition nextPosition)
        {
            Events = events;
            NextPosition = nextPosition;
        }

        public IEnumerable<IEventStoreRecord> Events { get; }
        public IStreamPosition NextPosition { get; }
    }
}
