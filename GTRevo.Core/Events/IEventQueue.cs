using GTRevo.Transactions;

namespace GTRevo.Events
{
    public interface IEventQueue : ITransactionProvider
    {
        void PushEvent(IEvent ev);
    }
}