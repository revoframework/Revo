using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Services;

namespace Revo.Core.Commands;

public class LocalCommandBus(
    IVariantServiceResolver serviceResolver,
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
            
        // variant resolution, so that a handler for a base command type can handle derived commands
        object commandHandler = serviceResolver.GetRequiredService(handlerType);
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