using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Infrastructure.Jobs
{
    public class JobRunner : IJobRunner
    {
        private readonly IServiceLocator serviceLocator;

        public JobRunner(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public Task RunJobAsync(IJob job)
        {
            Type jobHandlerType = typeof(IJobHandler<>).MakeGenericType(job.GetType());
            object jobHandler = serviceLocator.Get(jobHandlerType);
            var handleAsyncMethod = jobHandler.GetType().GetMethod(nameof(IJobHandler<IJob>.HandleAsync));
            var cancellationToken = new CancellationToken();
            return (Task)handleAsyncMethod.Invoke(jobHandler, new object[] {job, cancellationToken});
        }
    }
}
