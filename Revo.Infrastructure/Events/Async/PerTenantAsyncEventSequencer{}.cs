namespace Revo.Infrastructure.Events.Async
{
    public class PerTenantAsyncEventSequencer<TListener> : PerTenantAsyncEventSequencer
        where TListener : IAsyncEventListener
    {
        protected PerTenantAsyncEventSequencer() : base(typeof(TListener).Name)
        {
        }
    }
}
