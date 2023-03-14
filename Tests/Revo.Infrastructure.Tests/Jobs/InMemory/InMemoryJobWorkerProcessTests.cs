using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs.InMemory
{
    public class InMemoryJobWorkerProcessTests
    {
        private InMemoryJobWorkerProcess sut;
        private IJobRunner jobRunner;
        private InMemoryJobSchedulerConfiguration schedulerConfiguration;

        public InMemoryJobWorkerProcessTests()
        {
            jobRunner = Substitute.For<IJobRunner>();
            schedulerConfiguration = new InMemoryJobSchedulerConfiguration();

            sut = new InMemoryJobWorkerProcess(jobRunner, schedulerConfiguration,
                new NullLogger<InMemoryJobWorkerProcess>());
        }

        [Fact]
        public void EnqueueJob_RunsTask()
        {
            var job = new Job1();

            sut.OnApplicationStarted();
            sut.EnqueueJob(job, (j, e) => { });
            sut.OnApplicationStopping();

            jobRunner.Received(1).RunJobAsync(job, CancellationToken.None);
        }

        [Fact]
        public void ThrottlesTasks()
        {
            schedulerConfiguration.WorkerTaskParallelism = 2;
            
            bool job1Finish = false;
            bool job2Finish = false;

            var job1 = new Job1();
            var job2 = new Job1();
            var job3 = new Job1();

            async Task Job1()
            {
                while (!job1Finish)
                {
                    await Task.Yield();
                }
            }

            Lazy<Task> job1Task = new Lazy<Task>(Job1);
            
            async Task Job2()
            {
                while (!job2Finish)
                {
                    await Task.Yield();
                }
            }

            Lazy<Task> job2Task = new Lazy<Task>(Job2);

            async Task Job3()
            {
                job1Task.Value.IsCompleted.Should().BeTrue();
            }

            Lazy<Task> job3Task = new Lazy<Task>(Job3);

            jobRunner.RunJobAsync(job1, CancellationToken.None).Returns(ci => job1Task.Value);
            jobRunner.RunJobAsync(job2, CancellationToken.None).Returns(ci => job2Task.Value);
            jobRunner.RunJobAsync(job3, CancellationToken.None).Returns(ci => job3Task.Value);

            sut.OnApplicationStarted();
            sut.EnqueueJob(job1, (j, e) => { });
            sut.EnqueueJob(job2, (j, e) => { });

            while (!job1Task.IsValueCreated || !job2Task.IsValueCreated)
            {
                Thread.Yield();
            }

            sut.EnqueueJob(job3, (j, e) => { });

            Thread.Yield();

            job3Task.IsValueCreated.Should().BeFalse();

            job1Finish = true;
            job2Finish = true;
            sut.OnApplicationStopping();

            job3Task.Value.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public void CallsErrorHandler()
        {
            var job = new Job1();

            var exception = new Exception("Test exception");
            jobRunner.RunJobAsync(job, CancellationToken.None).Throws(exception);

            var errorHandler = Substitute.For<Action<IJob, Exception>>();

            sut.OnApplicationStarted();
            sut.EnqueueJob(job, errorHandler);
            sut.OnApplicationStopping();

            errorHandler.Received(1).Invoke(job, exception);
        }

        private class Job1 : IJob
        {
        }
    }
}
