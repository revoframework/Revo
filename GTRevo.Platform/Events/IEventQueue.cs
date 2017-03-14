using GTRevo.Platform.Transactions;

namespace GTRevo.Platform.Events
{
    public interface IEventQueue : ITransactionProvider
    {
        void PushEvent(IEvent ev);
    }
}