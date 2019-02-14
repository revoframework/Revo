using System;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
{
    public interface IJobScheduler
    {
        Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay = null);
        Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt);
        Task AddOrUpdateRecurringJobAsync(IJob job, string jobId, string cronExpression);
        Task RemoveRecurringJobIfExists(string jobId);
        Task DeleteScheduleJobAsync(string jobId);
        //void ScheduleRecurringJob(Job job, RecurringJobSchedule schedule);
    }
}
