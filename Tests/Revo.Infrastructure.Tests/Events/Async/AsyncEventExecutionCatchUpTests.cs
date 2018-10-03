using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Infrastructure.Events.Async;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventExecutionCatchUpTests
    {
        private AsyncEventExecutionCatchUp sut;
        private IEventSourceCatchUp[] eventSourceCatchUps;
        private IAsyncEventQueueManager asyncEventQueueManager;
        private List<IAsyncEventWorker> asyncEventQueueBacklogWorkers = new List<IAsyncEventWorker>();
        private List<(IAsyncEventWorker, string)> processedQueues = new List<(IAsyncEventWorker, string)>();

        public AsyncEventExecutionCatchUpTests()
        {
            eventSourceCatchUps = new[]
            {
                Substitute.For<IEventSourceCatchUp>(),
                Substitute.For<IEventSourceCatchUp>()
            };

            asyncEventQueueManager = Substitute.For<IAsyncEventQueueManager>();

            sut = new AsyncEventExecutionCatchUp(
                eventSourceCatchUps,
                asyncEventQueueManager,
                () =>
                {
                    var worker = Substitute.For<IAsyncEventWorker>();
                    worker.WhenForAnyArgs(x => x.RunQueueBacklogAsync(null)).Do(ci =>
                    {
                        lock (processedQueues)
                        {
                            processedQueues.Add((worker, ci.ArgAt<string>(0)));
                        }
                    });

                    lock (asyncEventQueueBacklogWorkers)
                    {
                        asyncEventQueueBacklogWorkers.Add(worker);
                    }

                    return worker;
                });
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
            
            processedQueues.Should().HaveCount(2);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("first", "second");
        }

        [Fact]
        public void OnApplicationStarted_RunsQueues()
        {
            asyncEventQueueManager.GetNonemptyQueueNamesAsync().Returns(
                new List<string>() { "throwing", "okay" });

            sut = new AsyncEventExecutionCatchUp(
                eventSourceCatchUps,
                asyncEventQueueManager,
                () =>
                {
                    var worker = Substitute.For<IAsyncEventWorker>();

                    worker.When(x => x.RunQueueBacklogAsync("okay")).Do(ci =>
                    {
                        lock (processedQueues)
                        {
                            processedQueues.Add((worker, ci.ArgAt<string>(0)));
                        }
                    });

                    worker.RunQueueBacklogAsync("throwing")
                        .Returns(Task.FromException(new AsyncEventProcessingException()));
                    return worker;
                });

            sut.OnApplicationStarted();

            processedQueues.Should().HaveCount(1);
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("okay");
        }
    }
}
