using System;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public class InMemoryJobScheduler : IInMemoryJobScheduler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IInMemoryJobSchedulerConfiguration schedulerConfiguration;
        private readonly IInMemoryJobWorkerProcess workerProcess;
        private readonly IInMemoryJobSchedulerProcess schedulerProcess;
        private readonly Random retryRandom = new Random();

        public InMemoryJobScheduler(IInMemoryJobSchedulerConfiguration schedulerConfiguration,
            IInMemoryJobWorkerProcess workerProcess, IInMemoryJobSchedulerProcess schedulerProcess)
        {
            this.schedulerConfiguration = schedulerConfiguration;
            this.workerProcess = workerProcess;
            this.schedulerProcess = schedulerProcess;
        }

        public Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay)
        {
            JobInfo jobInfo = new JobInfo(job);

            if (timeDelay == null)
            {
                workerProcess.EnqueueJob(job, (_, exception) => HandleError(jobInfo, exception));
            }
            else
            {
                var enqueueAt = Clock.Current.Now + timeDelay.Value;
                schedulerProcess.ScheduleJob(job, enqueueAt, (_, exception) => HandleError(jobInfo, exception));
            }

            return Task.FromResult<string>(null);
        }

        public Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt)
        {
            JobInfo jobInfo = new JobInfo(job);
            
            schedulerProcess.ScheduleJob(job, enqueueAt, (_, exception) => HandleError(jobInfo, exception));
            return Task.FromResult<string>(null);
        }

        public Task AddOrUpdateRecurringJobAsync(IJob job, string jobId, string cronExpression)
        {
            throw new NotImplementedException("InMemoryJobScheduler does not support recurring jobs");
        }

        public Task RemoveRecurringJobIfExists(string jobId)
        {
            throw new NotImplementedException("InMemoryJobScheduler does not support recurring jobs");
        }

        public Task DeleteScheduleJobAsync(string jobId)
        {
            throw new NotImplementedException("InMemoryJobScheduler does not support recurring jobs");
        }

        private void HandleError(JobInfo jobInfo, Exception exception)
        {
            Logger.Error(exception, $"Error processing job: {jobInfo.Job.ToString()} (attempt #{jobInfo.AttemptNumber})");

            int Pow(int bas, int exp)
            {
                return Enumerable
                    .Repeat(bas, exp)
                    .Aggregate(1, (a, b) => a * b);
            }

            if (jobInfo.AttemptNumber < schedulerConfiguration.HandleAttemptCount)
            {
                int delayMultiplier = Pow(schedulerConfiguration.HandleRetryTimeoutMultiplier, jobInfo.AttemptNumber - 1);
                long minDelay = schedulerConfiguration.MinHandleRetryTimeoutStep.Ticks * delayMultiplier;
                long maxDelay = schedulerConfiguration.MaxHandleRetryTimeoutStep.Ticks * delayMultiplier;
                TimeSpan delay = TimeSpan.FromTicks(minDelay + retryRandom.Next(0, (int) (maxDelay - minDelay)));

                DateTimeOffset enqueueAt = Clock.Current.Now + delay;

                jobInfo.AttemptNumber++;

                Logger.Info($"Scheduling job retry for {jobInfo.Job.ToString()} in {Math.Ceiling(delay.TotalSeconds)} seconds (attempt #{jobInfo.AttemptNumber})");
                schedulerProcess.ScheduleJob(jobInfo.Job, enqueueAt, (_, newException) => HandleError(jobInfo, newException));
            }
        }

        private class JobInfo
        {
            public JobInfo(IJob job)
            {
                Job = job;
            }

            public IJob Job { get; }
            public int AttemptNumber { get; set; } = 1;
        }
    }
}
