using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class FakeAsyncEventQueueState : IAsyncEventQueueState
    {
        public long? LastSequenceNumberProcessed { get; set; }
    }
}
