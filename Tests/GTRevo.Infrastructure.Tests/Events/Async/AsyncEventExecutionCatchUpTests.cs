using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Events.Async;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventExecutionCatchUpTests
    {
        private AsyncEventExecutionCatchUp sut;
        private IEventSourceCatchUp[] eventSourceCatchUps;
        private IAsyncEventQueueManager asyncEventQueueManager;
        private IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;

        public AsyncEventExecutionCatchUpTests()
        {
            eventSourceCatchUps = new[]
            {
                Substitute.For<IEventSourceCatchUp>(),
                Substitute.For<IEventSourceCatchUp>()
            };

            asyncEventQueueManager = Substitute.For<IAsyncEventQueueManager>();
            asyncEventQueueBacklogWorker = Substitute.For<IAsyncEventQueueBacklogWorker>();

            sut = new AsyncEventExecutionCatchUp(
                eventSourceCatchUps,
                asyncEventQueueManager,
                asyncEventQueueBacklogWorker);
        }

        [Fact]
        public void OnApplicationStarted_InvokesCatchups()
        {
            List<string> nonEmptyQueueNames = new List<string>();

            eventSourceCatchUps[0].When(x => x.CatchUpAsync())
                .Do(ci =>
                {
                    nonEmptyQueueNames.Add("first");
                });

            eventSourceCatchUps[1].When(x => x.CatchUpAsync())
                .Do(ci =>
                {
                    nonEmptyQueueNames.Add("second");
                });

            asyncEventQueueManager.GetNonemptyQueueNamesAsync().Returns(nonEmptyQueueNames);

            sut.OnApplicationStarted();

            asyncEventQueueBacklogWorker.Received(1).RunQueueBacklogAsync("first");
            asyncEventQueueBacklogWorker.Received(1).RunQueueBacklogAsync("second");
        }
    }
}
