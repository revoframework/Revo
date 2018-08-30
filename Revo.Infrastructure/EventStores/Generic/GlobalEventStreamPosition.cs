namespace Revo.Infrastructure.EventStores.Generic
{
    public class GlobalEventStreamPosition : IStreamPosition
    {
        public GlobalEventStreamPosition(long lastGlobalSequenceNumberRead)
        {
            LastGlobalSequenceNumberRead = lastGlobalSequenceNumberRead;
        }

        public long LastGlobalSequenceNumberRead { get; }
    }
}
