using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Commands;

public class LocalCommandBus(
    IServiceProvider serviceProvider,
    ICommandBusPipeline pipeline)
    : ILocalCommandBus
{
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        object result = await pipeline.ProcessAsync(command, ExecutionHandler<TResult>, this,
            executionOptions, cancellationToken);
        return (TResult) result;
    }

    public Task SendAsync(ICommandBase command, CancellationToken cancellationToken)
    {
        return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
    }

    public async Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        await pipeline.ProcessAsync(command, ExecutionHandler<object>, this,
            executionOptions, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        return SendAsync<TResult>(command, CommandExecutionOptions.Default, cancellationToken);
    }
        
    private async Task<object> ExecutionHandler<TResult>(
        ICommandBase command, CancellationToken cancellationToken)
    {
        if (command == null)
        {
            throw new ArgumentNullException(nameof(command));
        }

        Type commandType = command.GetType();
            
        Type genericCommandType = command.GetType().GetInterfaces().FirstOrDefault(
            x => x.IsConstructedGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));
        Type handlerType;

        if (genericCommandType != null)
        {
            handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(),
                genericCommandType.GetGenericArguments()[0]);
        }
        else
        {
            handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        }
            
        object commandHandler = serviceProvider.GetRequiredService(handlerType);
        var handleMethod = commandHandler.GetType()
            .GetRuntimeMethod(nameof(ICommandHandler<ICommand>.HandleAsync), [commandType, typeof(CancellationToken)
            ]);

        var resultTask = (Task) handleMethod.Invoke(commandHandler, [command, cancellationToken]);

        if (resultTask is Task<TResult> resultObjectTask)
        {
            return await resultObjectTask;
        }
        else
        {
            await resultTask;
            return default;
        }
    }
}