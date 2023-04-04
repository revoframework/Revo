using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Commands.Lambda;

public class LambdaCommandHandler(IServiceProvider serviceProvider) :
    ICommandHandler<LambdaCommand>,
    ICommandHandler<LambdaResultCommand, object>
{
    public async Task HandleAsync(LambdaCommand command, CancellationToken cancellationToken)
    {
        var lambdaParams = command.Delegate.Method.GetParameters()
            .Select(x => serviceProvider.GetRequiredService(x.ParameterType))
            .ToArray();
            
        var result = command.Delegate.DynamicInvoke(lambdaParams);
        if (result is Task resultTask)
        {
            await resultTask;
        }
    }

    public async Task<object> HandleAsync(LambdaResultCommand query, CancellationToken cancellationToken)
    {
        var lambdaParams = query.Delegate.Method.GetParameters()
            .Select(x => serviceProvider.GetRequiredService(x.ParameterType))
            .ToArray();

        var result = query.Delegate.DynamicInvoke(lambdaParams);
        if (result is Task resultTask)
        {
            await resultTask;
            var resultValue = resultTask.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(resultTask);
            return resultValue;
        }
        else
        {
            return result;
        }
    }
}