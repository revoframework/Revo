namespace Revo.Infrastructure.EventStores.Generic
{
    public class GlobalEventStreamPosition(long lastGlobalSequenceNumberRead) : IStreamPosition
    {
        public long LastGlobalSequenceNumberRead { get; } = lastGlobalSequenceNumberRead;
    }
}
