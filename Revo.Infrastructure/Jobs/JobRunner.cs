using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Infrastructure.Jobs;

public class JobRunner(IServiceProvider serviceProvider) : IJobRunner
{
    public Task RunJobAsync(IJob job, CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        Type jobHandlerType = typeof(IJobHandler<>).MakeGenericType(job.GetType());
        object jobHandler = scope.ServiceProvider.GetRequiredService(jobHandlerType);
        var handleAsyncMethod = jobHandlerType.GetMethod(nameof(IJobHandler<IJob>.HandleAsync));
        return (Task)handleAsyncMethod.Invoke(jobHandler, new object[] {job, cancellationToken});
    }
}