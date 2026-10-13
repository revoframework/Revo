using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory serviceScopeFactory;

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
                CatchUpProcessingParallelism = 80,
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

            serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(_ =>
            {
                var scopedProvider = Substitute.For<IServiceProvider>();
                scopedProvider.GetService(typeof(IEnumerable<IEventSourceCatchUp>)).Returns(eventSourceCatchUps);
                scopedProvider.GetService(typeof(IAsyncEventQueueManager)).Returns(asyncEventQueueManager);

                IAsyncEventWorker worker = null;
                scopedProvider.GetService(typeof(IAsyncEventWorker)).Returns(_ =>
                {
                    if (worker != null)
                    {
                        return worker;
                    }

                    worker = Substitute.For<IAsyncEventWorker>();
                    worker.WhenForAnyArgs(x => x.RunQueueBacklogAsync(null)).Do(innerCi =>
                    {
                        lock (processedQueues)
                        {
                            processedQueues.Add((worker, innerCi.ArgAt<string>(0)));
                        }
                    });

                    lock (asyncEventQueueBacklogWorkers)
                    {
                        asyncEventQueueBacklogWorkers.Add(worker);
                    }

                    return worker;
                });

                var scope = Substitute.For<IServiceScope>();
                scope.ServiceProvider.Returns(scopedProvider);
                return scope;
            });

            sut = new AsyncEventExecutionCatchUp(
                asyncEventPipelineConfiguration,
                serviceScopeFactory,
                new NullLogger<AsyncEventExecutionCatchUp>());
        }

        [Fact]
        public async Task StartedAsync_InvokesCatchups()
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

            await sut.StartedAsync(CancellationToken.None);
            
            processedQueues.Should().HaveCount(2);
            processedQueues.Select(x => x.Item1).Should().BeEquivalentTo(asyncEventQueueBacklogWorkers, cfg => cfg.ComparingByValue<IAsyncEventWorker>());
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("first", "second");
        }

        [Fact]
        public async Task StartedAsync_RunsQueues()
        {
            asyncEventQueueManager.GetNonemptyQueueNamesAsync().Returns(
                new List<string>() { "throwing", "okay" });

            var worker = Substitute.For<IAsyncEventWorker>();
            worker.When(x => x.RunQueueBacklogAsync("okay")).Do(innerCi =>
            {
                lock (processedQueues)
                {
                    processedQueues.Add((worker, innerCi.ArgAt<string>(0)));
                }
            });
            worker.RunQueueBacklogAsync("throwing")
                .Returns(Task.FromException(new AsyncEventProcessingException()));

            var testScopeFactory = Substitute.For<IServiceScopeFactory>();
            testScopeFactory.CreateScope().Returns(_ =>
            {
                var scopedProvider = Substitute.For<IServiceProvider>();
                scopedProvider.GetService(typeof(IEnumerable<IEventSourceCatchUp>)).Returns(eventSourceCatchUps);
                scopedProvider.GetService(typeof(IAsyncEventQueueManager)).Returns(asyncEventQueueManager);
                scopedProvider.GetService(typeof(IAsyncEventWorker)).Returns(worker);

                var scope = Substitute.For<IServiceScope>();
                scope.ServiceProvider.Returns(scopedProvider);
                return scope;
            });

            sut = new AsyncEventExecutionCatchUp(
                asyncEventPipelineConfiguration,
                testScopeFactory,
                new NullLogger<AsyncEventExecutionCatchUp>());

            await sut.StartedAsync(CancellationToken.None);

            processedQueues.Should().HaveCount(1);
            processedQueues.Select(x => x.Item2).Should().BeEquivalentTo("okay");
        }
    }
}