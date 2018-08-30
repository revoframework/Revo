using System.Collections.Generic;

namespace Revo.Infrastructure.EventStores
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
