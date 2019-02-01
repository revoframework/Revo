using System;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public interface IInMemoryJobSchedulerProcess
    {
        void ScheduleJob(IJob job, DateTimeOffset enqueueAt, Action<IJob, Exception> errorHandler);
    }
}