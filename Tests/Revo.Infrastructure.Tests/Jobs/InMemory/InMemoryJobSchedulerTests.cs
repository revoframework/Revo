using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;
using Revo.Testing.Core;
using Xunit;

namespace Revo.Infrastructure.Tests.Jobs.InMemory
{
    public class InMemoryJobSchedulerTests
    {
        private InMemoryJobScheduler sut;
        private InMemoryJobSchedulerConfiguration schedulerConfiguration;
        private IInMemoryJobWorkerProcess workerProcess;
        private IInMemoryJobSchedulerProcess schedulerProcess;

        public InMemoryJobSchedulerTests()
        {
            schedulerConfiguration = new InMemoryJobSchedulerConfiguration();
            workerProcess = Substitute.For<IInMemoryJobWorkerProcess>();
            schedulerProcess = Substitute.For<IInMemoryJobSchedulerProcess>();

            sut = new InMemoryJobScheduler(schedulerConfiguration, workerProcess, schedulerProcess,
                new NullLogger<InMemoryJobScheduler>());
        }

        [Fact]
        public async Task EnqeueJobAsync_Immediate()
        {
            var job = new Job1();
            await sut.EnqeueJobAsync(job, null);
            workerProcess.Received(1).EnqueueJob(job, Arg.Any<Action<IJob, Exception>>());
        }

        [Fact]
        public async Task EnqeueJobAsync_Delayed()
        {
            FakeClock.Setup();

            var job = new Job1();
            await sut.EnqeueJobAsync(job, TimeSpan.FromSeconds(5));

            var expectedScheduleTime = FakeClock.Now + TimeSpan.FromSeconds(5);

            schedulerProcess.Received(1).ScheduleJob(job, expectedScheduleTime, Arg.Any<Action<IJob, Exception>>());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public async Task ReschedulesOnError(int configMaxAttemptCount)
        {
            schedulerConfiguration.HandleAttemptCount = configMaxAttemptCount;

            FakeClock.Setup();
            FakeClock.Now = new DateTimeOffset(2019, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var job = new Job1();

            Action<IJob, Exception> errorHandler = null;

            workerProcess.When(x => x.EnqueueJob(job, Arg.Any<Action<IJob, Exception>>()))
                .Do(ci =>
                {
                    errorHandler = ci.ArgAt<Action<IJob, Exception>>(1);
                });

            DateTimeOffset scheduleTime = DateTimeOffset.MinValue;

            schedulerProcess.When(x => x.ScheduleJob(job, Arg.Any<DateTimeOffset>(), Arg.Any<Action<IJob, Exception>>()))
                .Do(ci =>
                {
                    scheduleTime = ci.ArgAt<DateTimeOffset>(1);
                    errorHandler = ci.ArgAt<Action<IJob, Exception>>(2);
                });

            await sut.EnqeueJobAsync(job, null);

            int Pow(int bas, int exp)
            {
                return Enumerable
                    .Repeat(bas, exp)
                    .Aggregate(1, (a, b) => a * b);
            }

            for (int numberAttempts = 1; numberAttempts <= configMaxAttemptCount; numberAttempts++)
            {
                errorHandler.Should().NotBeNull();

                var currentErrorHandler = errorHandler;
                errorHandler = null;

                var exception = new Exception();
                currentErrorHandler(job, exception);

                schedulerProcess.Received(Math.Min(numberAttempts, configMaxAttemptCount - 1))
                    .ScheduleJob(job, Arg.Any<DateTimeOffset>(), Arg.Any<Action<IJob, Exception>>());

                if (numberAttempts != configMaxAttemptCount) // should retry
                {
                    int delayMultiplier = Pow(schedulerConfiguration.HandleRetryTimeoutMultiplier, numberAttempts - 1);
                    long minDelay = schedulerConfiguration.MinHandleRetryTimeoutStep.Ticks * delayMultiplier;
                    long maxDelay = schedulerConfiguration.MaxHandleRetryTimeoutStep.Ticks * delayMultiplier;

                    scheduleTime.Should().BeOnOrAfter(FakeClock.Now + TimeSpan.FromTicks(minDelay));
                    scheduleTime.Should().BeOnOrBefore(FakeClock.Now + TimeSpan.FromTicks(maxDelay));
                }
            }

            workerProcess.Received(1).EnqueueJob(job, Arg.Any<Action<IJob, Exception>>()); // only once
        }

        private class Job1 : IJob
        {
        }
    }
}
