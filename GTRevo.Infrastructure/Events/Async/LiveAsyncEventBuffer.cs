using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events.Async
{
    public class LiveAsyncEventBuffer
    {
        private readonly List<IEventMessage> events = new List<IEventMessage>();
        
        public void AddEvents(IReadOnlyCollection<IEventMessage> events)
        {
            this.events.AddRange(events);
        }

        public void DequeueEvents(int numberOfEvents)
        {
            if (numberOfEvents > 0)
            {
                events.RemoveRange(0, numberOfEvents);
            }
        }

        public void Lock()
        {
            Monitor.Enter(this);
        }

        public void Unlock()
        {
            Monitor.Exit(this);
        }
    }
}
