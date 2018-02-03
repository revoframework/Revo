using System;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
{
    public interface IJobScheduler
    {
        Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay);
        Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt);
        Task DeleteJobScheduleAsync(string jobId);
        //void ScheduleRecurringJob(Job job, RecurringJobSchedule schedule);
    }
}
