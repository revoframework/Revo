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

        public Task RunJobAsync(IJob job, CancellationToken cancellationToken)
        {
            Type jobHandlerType = typeof(IJobHandler<>).MakeGenericType(job.GetType());
            object jobHandler = serviceLocator.Get(jobHandlerType);
            var handleAsyncMethod = jobHandlerType.GetMethod(nameof(IJobHandler<IJob>.HandleAsync));
            return (Task)handleAsyncMethod.Invoke(jobHandler, new object[] {job, cancellationToken});
        }
    }
}
