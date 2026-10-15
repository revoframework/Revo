using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Services;

namespace Revo.Infrastructure.Jobs;

public class JobRunner(IServiceProvider serviceProvider) : IJobRunner
{
    public async Task RunJobAsync(IJob job, CancellationToken cancellationToken)
    {
        // the scope must stay alive until the job has finished, hence the await
        using var scope = serviceProvider.CreateScope();

        Type jobHandlerType = typeof(IJobHandler<>).MakeGenericType(job.GetType());
        // variant resolution, so that e.g. IJobHandler<IExecuteCommandJob> handles ExecuteCommandJob
        object jobHandler = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>()
            .GetRequiredService(jobHandlerType);
        var handleAsyncMethod = jobHandlerType.GetMethod(nameof(IJobHandler<IJob>.HandleAsync));
        await (Task)handleAsyncMethod.Invoke(jobHandler, new object[] {job, cancellationToken});
    }
}