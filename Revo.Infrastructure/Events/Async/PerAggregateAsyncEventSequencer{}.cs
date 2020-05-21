namespace Revo.Infrastructure.Events.Async
{
    public class PerAggregateAsyncEventSequencer<TListener> : PerAggregateAsyncEventSequencer
        where TListener : IAsyncEventListener
    {
        public PerAggregateAsyncEventSequencer() : base(typeof(TListener).Name)
        {
        }
    }
}
