using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Jobs;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events.Async
{
    public class ProcessAsyncEventsJobHandlerTests
    {
        private ProcessAsyncEventsJobHandler sut;
        private IAsyncEventQueueBacklogWorker asyncEventQueueBacklogWorker;
        private IJobScheduler jobScheduler;
        private AsyncEventPipelineConfiguration asyncEventPipelineConfiguration;

        public ProcessAsyncEventsJobHandlerTests()
        {
            asyncEventQueueBacklogWorker = Substitute.For<IAsyncEventQueueBacklogWorker>();
            jobScheduler = Substitute.For<IJobScheduler>();
            asyncEventPipelineConfiguration = new AsyncEventPipelineConfiguration();

            sut = new ProcessAsyncEventsJobHandler(asyncEventQueueBacklogWorker, asyncEventPipelineConfiguration, jobScheduler);
        }

        [Fact]
        public async Task HandleAsync_RunsSuccessfully()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            await sut.HandleAsync(job, CancellationToken.None);

            asyncEventQueueBacklogWorker.Received(1).RunQueueBacklogAsync(job.QueueName);
        }

        [Fact]
        public async Task HandleAsync_RetryAsyncEventProcessingSequenceException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            asyncEventQueueBacklogWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new AsyncEventProcessingSequenceException(0));
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.Received(1).EnqeueJobAsync(Arg.Is<ProcessAsyncEventsJob>(x => x.AttemptsLeft == 4
                && x.RetryTimeout == TimeSpan.FromTicks(job.RetryTimeout.Ticks * asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier)
                && x.QueueName == "queue"), job.RetryTimeout);
        }

        [Fact]
        public async Task HandleAsync_NoRetriesLeftForAsyncEventProcessingSequenceException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 1, TimeSpan.FromMinutes(1));
            asyncEventQueueBacklogWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new AsyncEventProcessingSequenceException(0));
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.DidNotReceiveWithAnyArgs().EnqeueJobAsync(null, null);
        }

        [Fact]
        public async Task HandleAsync_RetryOptimisticConcurrencyException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 5, TimeSpan.FromMinutes(1));
            asyncEventQueueBacklogWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new OptimisticConcurrencyException());
            
            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.Received(1).EnqeueJobAsync(Arg.Is<ProcessAsyncEventsJob>(x => x.AttemptsLeft == 4
                && x.RetryTimeout == TimeSpan.FromTicks(job.RetryTimeout.Ticks * asyncEventPipelineConfiguration.AsyncProcessRetryTimeoutMultiplier)
                && x.QueueName == "queue"), job.RetryTimeout);
        }


        [Fact]
        public async Task HandleAsync_NoRetriesLeftForOptimisticConcurrencyException()
        {
            ProcessAsyncEventsJob job = new ProcessAsyncEventsJob("queue", 1, TimeSpan.FromMinutes(1));
            asyncEventQueueBacklogWorker.When(x => x.RunQueueBacklogAsync(job.QueueName)).Throw(new OptimisticConcurrencyException());

            await sut.HandleAsync(job, CancellationToken.None);

            jobScheduler.DidNotReceiveWithAnyArgs().EnqeueJobAsync(null, null);
        }
    }
}
