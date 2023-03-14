using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.Infrastructure.Events.Async;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class AsyncEventExecutionCatchUpTests
    {
        private AsyncEventExecutionCatchUp sut;
        private readonly IEventSourceCatchUp[] eventSourceCatchUps;
        private readonly IAsyncEventQueueManager asyncEventQueueManager;
        private readonly List<IAsyncEventWorker> asyncEventQueueBacklogWorkers = new List<IAsyncEventWorker>();
        private readonly List<(IAsyncEventWorker, string)> processedQueues = new List<(IAsyncEventWorker, string)>();
        private readonly AsyncEventPipelineConfiguration asyncEventPipelineConfiguration;

        public AsyncEventExecutionCatchUpTests()
        {
            eventSourceCatchUps = new[]
            {
                Substitute.For<IEventSourceCatchUp>(),
                Substitute.For<IEventSourceCatchUp>()
            };

            asyncEventQueueManager = Substitute.For<IAsyncEventQueueManager>();
            asyncEventPipelineConfiguration = new AsyncEventPipelineConfiguration()
            {
                CatchUpProcessingParallelism = 80, // TODO test this
                SyncQueueProcessingParallelism = 5,
                AsyncProcessAttemptCount = 3,
                SyncProcessAttemptCount = 3,
                AsyncRescheduleDelayAfterSyncProcessFailure = TimeSpan.FromMinutes(1),
                AsyncProcessRetryTimeout = TimeSpan.FromMilliseconds(500),
                AsyncProcessRetryTimeoutMultiplier = 6,
                SyncProcessRetryTimeout = TimeSpan.FromMilliseconds(600),
                SyncProcessRetryTimeoutMultiplier = 4,
                WaitForEventCatchUpsUponStartup = true
            };

            sut = new AsyncEventExecutionCatchUp(
                eventSourceCatchUps,
                asyncEventQueueManager,
                asyncEventPipelineConfiguration,
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
                },
                new NullLogger<AsyncEventExecutionCatchUp>());
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
                asyncEventPipelineConfiguration,
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
                },
                new NullLogger<AsyncEventExecutionCatchUp>());

            sut.OnApplicationStarted();

            processedQueues.Should().HaveCount(1);
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("okay");
        }
    }
}
