using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events.Async;
using NSubstitute;
using Revo.Infrastructure.Jobs.InMemory;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class ProcessAsyncEventsJobHandlerTests
    {
        private ProcessAsyncEventsJobHandler sut;
        private IAsyncEventWorker asyncEventWorker;
        private IInMemoryJobScheduler jobScheduler;
        private AsyncEventPipelineConfiguration asyncEventPipelineConfiguration;

        public ProcessAsyncEventsJobHandlerTests()
        {
            asyncEventWorker = Substitute.For<IAsyncEventWorker>();
            jobScheduler = Substitute.For<IInMemoryJobScheduler>();
            asyncEventPipelineConfiguration = new AsyncEventPipelineConfiguration();

            sut = new ProcessAsyncEventsJobHandler(asyncEventWorker, asyncEventPipelineConfiguration,
                jobScheduler, new NullLogger<ProcessAsyncEventsJobHandler>());
        }

        [Fact]
        public async Task HandleAsync_RunsSuccessfully()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            await sut.HandleAsync(job, CancellationToken.None);

            asyncEventWorker.Received(1).RunQueueBacklogAsync(job.QueueName);
        }

        [Fact]
        public async Task HandleAsync_RetryAsyncEventProcessingSequenceException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            asyncEventWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new AsyncEventProcessingSequenceException(0));
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.Received(1).EnqeueJobAsync(Arg.Is<ProcessAsyncEventsJob>(x => x.AttemptsLeft == 4
                && x.RetryTimeout == TimeSpan.FromTicks(job.RetryTimeout.Ticks * asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier)
                && x.QueueName == "queue"), job.RetryTimeout);
        }

        [Fact]
        public async Task HandleAsync_NoRetriesLeftForAsyncEventProcessingSequenceException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 1, TimeSpan.FromMinutes(1));
            asyncEventWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new AsyncEventProcessingSequenceException(0));
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.DidNotReceiveWithAnyArgs().EnqeueJobAsync(null, null);
        }

        [Fact]
        public async Task HandleAsync_RetryOptimisticConcurrencyException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            asyncEventWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new OptimisticConcurrencyException());
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.Received(1).EnqeueJobAsync(Arg.Is<ProcessAsyncEventsJob>(x => x.AttemptsLeft == 4
                && x.RetryTimeout == TimeSpan.FromTicks(job.RetryTimeout.Ticks * asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier)
                && x.QueueName == "queue"), job.RetryTimeout);
        }


        [Fact]
        public async Task HandleAsync_NoRetriesLeftForOptimisticConcurrencyException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 1, TimeSpan.FromMinutes(1));
            asyncEventWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new OptimisticConcurrencyException());

            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.DidNotReceiveWithAnyArgs().EnqeueJobAsync(null, null);
        }
    }
}
