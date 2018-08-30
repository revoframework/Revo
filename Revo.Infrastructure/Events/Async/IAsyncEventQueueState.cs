namespace Revo.Infrastructure.Events.Async
{
    public interface IAsyncEventQueueState
    {
        long? LastSequenceNumberProcessed { get; }
    }
}
