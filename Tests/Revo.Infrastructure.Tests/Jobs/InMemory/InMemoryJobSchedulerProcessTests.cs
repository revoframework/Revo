using System;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;
using Revo.Testing.Core;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs.InMemory
{
    public class InMemoryJobSchedulerProcessTests
    {
        private InMemoryJobSchedulerProcess sut;
        private IInMemoryJobWorkerProcess workerProcess;

        public InMemoryJobSchedulerProcessTests()
        {
            workerProcess = Substitute.For<IInMemoryJobWorkerProcess>();

            sut = new InMemoryJobSchedulerProcess(workerProcess);
        }

        [Fact]
        public void EnqueuesAtScheduledTime()
        {
            var job = new Job1();

            FakeClock.Setup();
            FakeClock.Now = new DateTimeOffset(2018, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var enqueueAt = FakeClock.Now + TimeSpan.FromMilliseconds(10);

            Action<IJob, Exception> errorHandler = (j, e) => { };

            sut.OnApplicationStarted();
            sut.ScheduleJob(job, enqueueAt, errorHandler);

            bool enqueued = false;
            workerProcess.When(x => x.EnqueueJob(job, errorHandler)).Do(ci => enqueued = true);

            enqueued.Should().BeFalse();

            FakeClock.Now = new DateTimeOffset(2018, 1, 1, 0, 0, 0, 10, TimeSpan.Zero);

            while (!enqueued)
            {
                Thread.Yield();
            }

            enqueued.Should().BeTrue();
            sut.OnApplicationStopping();
        }

        private class Job1 : IJob
        {
        }
    }
}
