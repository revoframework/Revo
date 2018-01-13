using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Events.Async;

namespace GTRevo.Infrastructure.Tests.Events.Async
{
    public class FakeAsyncEventQueueState : IAsyncEventQueueState
    {
        public long? LastSequenceNumberProcessed { get; set; }
    }
}
