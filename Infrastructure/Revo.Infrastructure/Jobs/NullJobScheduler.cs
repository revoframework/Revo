using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Jobs
{
    public class NullJobScheduler : IJobScheduler
    {
        public Task<string> EnqeueJobAsync(IJob job, TimeSpan? timeDelay)
        {
            throw new NotImplementedException("Cannot use NullJobScheduler to schedule jobs. Please configure a real job scheduler, e.g. Hangfire.");
        }

        public Task<string> ScheduleJobAsync(IJob job, DateTimeOffset enqueueAt)
        {
            throw new NotImplementedException("Cannot use NullJobScheduler to schedule jobs. Please configure a real job scheduler, e.g. Hangfire.");
        }

        public Task DeleteJobScheduleAsync(string jobId)
        {
            throw new NotImplementedException("Cannot use NullJobScheduler to schedule jobs. Please configure a real job scheduler, e.g. Hangfire.");
        }
    }
}
