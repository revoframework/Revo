using System;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire;

namespace Revo.Infrastructure.Jobs.Hangfire
{
    public class HangfireJobScheduler : IJobScheduler
    {
        public Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay)
        {
            var method = GetType().GetMethod(nameof(DoEnqeueJobAsync), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(job.GetType());
            return (Task<string>)genericMethod.Invoke(this, new object[] {job, timeDelay});
        }

        public Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt)
        {
            var method = GetType().GetMethod(nameof(DoScheduleJobAsync), BindingFlags.Instance | BindingFlags.NonPublic);
            var genericMethod = method.MakeGenericMethod(job.GetType());
            return (Task<string>)genericMethod.Invoke(this, new object[] { job, enqueueAt });
        }

        private Task<string> DoEnqeueJobAsync<TJob>(TJob job, TimeSpan? timeDelay)
            where TJob : IJob
        {
            string jobId = timeDelay == null
                ? BackgroundJob.Enqueue<HangfireJobEntryPoint<TJob>>(entryPoint => entryPoint.ExecuteAsync(job))
                : BackgroundJob.Schedule<HangfireJobEntryPoint<TJob>>(entryPoint => entryPoint.ExecuteAsync(job),
                    timeDelay.Value);

            return Task.FromResult(jobId);
        }

        private Task<string> DoScheduleJobAsync<TJob>(TJob job, DateTimeOffset enqueueAt)
            where TJob : IJob
        {
            string jobId = BackgroundJob.Schedule<HangfireJobEntryPoint<TJob>>(
                entryPoint => entryPoint.ExecuteAsync(job),
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
