using Revo.Infrastructure.EventStore;

namespace Revo.EF6.EventStore
{
    public class GlobalEF6EventStreamPosition : IStreamPosition
    {
        public GlobalEF6EventStreamPosition(long lastGlobalSequenceNumberRead)
        {
            LastGlobalSequenceNumberRead = lastGlobalSequenceNumberRead;
        }

        public long LastGlobalSequenceNumberRead { get; }
    }
}
