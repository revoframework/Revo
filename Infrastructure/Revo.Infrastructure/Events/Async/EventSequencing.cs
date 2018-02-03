namespace Revo.Infrastructure.Events.Async
{
    public struct EventSequencing
    {
        public string SequenceName { get; set; }
        public long? EventSequenceNumber { get; set; }
    }
}
