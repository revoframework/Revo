using Revo.Infrastructure.Events.Async;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class FakeAsyncEventQueueState : IAsyncEventQueueState
    {
        public long? LastSequenceNumberProcessed { get; set; }
    }
}
