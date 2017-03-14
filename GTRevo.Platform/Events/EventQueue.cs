using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Platform.Transactions;

namespace GTRevo.Platform.Events
{
    public class EventQueue : IEventQueue, IUnitOfWorkListener
    {
        private readonly IEventBus eventBus;
        private readonly Func<IEventQueueTransactionListener[]> listeners;
        private readonly Stack<EventQueueTransaction> transactions = new Stack<EventQueueTransaction>();

        public EventQueue(IEventBus eventBus,
            Func<IEventQueueTransactionListener[]> listeners)
        {
            this.eventBus = eventBus;
            this.listeners = listeners;
        }

        public ITransaction CreateTransaction()
        {
            var tx = new EventQueueTransaction(this);
            transactions.Push(tx);

            foreach (var listener in listeners())
            {
                listener.OnTransactionBeginned(tx);
            }

            return tx;
        }

        public void OnTransactionBeginned(ITransaction transaction)
        {
            CreateTransaction();
        }

        public Task OnTransactionSucceededAsync(ITransaction transaction)
        {
            return transactions.Peek().CommitAsync();
        }

        public void PushEvent(IEvent ev)
        {
            if (transactions.Count == 0)
            {
                throw new InvalidOperationException("No open event queue transactions");
            }

            transactions.Peek().PushEvent(ev);
        }

        internal async Task CommitTransactionAsync(EventQueueTransaction transaction)
        {
            try
            {
                do
                { }
                while (transactions.Pop() != transaction); //pop uncommited transactions
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException("Event queue transaction not found");
            }

            foreach (IEvent ev in transaction.Events) // TODO exception handling (i.e. throw away the remaining events as well?)
            {
                /* mediatr event publishing sucks and infers event type only
                 * from the generic parameter, so we actually have to use reflection
                 * (ca't use dynamic dispatch eiter because it doesn't play well generics */

                //await eventBus.Publish((dynamic)ev, default(CancellationToken));

                // TODO optimize
                var method = eventBus.GetType().GetMethod("Publish").MakeGenericMethod(ev.GetType());
                await ((Task)method.Invoke(eventBus, new object[] { ev, default(CancellationToken) }));
            }

            foreach (var listener in listeners())
            {
                await listener.OnTransactionSucceededAsync(transaction); // TODO what if failed or previous txs got rolled back?
            }
        }
    }
}
