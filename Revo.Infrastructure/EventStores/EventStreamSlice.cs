using System.Collections.Generic;

namespace Revo.Infrastructure.EventStores
{
    public class EventStreamSlice(IEnumerable<IEventStoreRecord> events, IStreamPosition nextPosition)
    {
        public IEnumerable<IEventStoreRecord> Events { get; } = events;
        public IStreamPosition NextPosition { get; } = nextPosition;
    }
}
