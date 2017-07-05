using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Core.Events
{
    public class EventQueueTransaction : ITransaction
    {
        private readonly EventQueue eventQueue;
        private List<IEvent> events = new List<IEvent>();

        public EventQueueTransaction(EventQueue eventQueue)
        {
            this.eventQueue = eventQueue;
        }

        public IEnumerable<IEvent> Events
        {
            get { return events; }
        }

        public void PushEvent(IEvent ev)
        {
            events.Add(ev);
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync()
        {
            return eventQueue.CommitTransactionAsync(this);
        }

        public void Dispose()
        {
        }
    }
}
