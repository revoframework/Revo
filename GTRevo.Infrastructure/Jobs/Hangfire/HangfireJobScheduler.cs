using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace GTRevo.Infrastructure.Jobs.Hangfire
{
    public class HangfireJobScheduler : IJobScheduler
    {
        public Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay)
        {
            string jobId = timeDelay == null
                ? BackgroundJob.Enqueue<IJobRunner>(jobRunner => jobRunner.RunJobAsync(job))
                : BackgroundJob.Schedule<IJobRunner>(jobRunner => jobRunner.RunJobAsync(job),
                    timeDelay.Value);

            return Task.FromResult(jobId);
        }

        public Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt)
        {
            string jobId = BackgroundJob.Schedule<IJobRunner>(
                jobRunner => jobRunner.RunJobAsync(job),
                enqueueAt);
            return Task.FromResult(jobId);
        }

        public Task DeleteJobScheduleAsync(string jobId)
        {
            BackgroundJob.Delete(jobId);
            return Task.FromResult(0);
        }
    }
}
