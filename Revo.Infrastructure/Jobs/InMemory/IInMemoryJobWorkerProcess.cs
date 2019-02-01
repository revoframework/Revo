using System;

namespace Revo.Infrastructure.Jobs.InMemory
{
    public interface IInMemoryJobWorkerProcess
    {
        void EnqueueJob(IJob job, Action<IJob, Exception> errorHandler);
    }
}