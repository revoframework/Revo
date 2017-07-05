using GTRevo.Core.Transactions;

namespace GTRevo.Core.Events
{
    public interface IEventQueue : ITransactionProvider
    {
        void PushEvent(IEvent ev);
    }
}