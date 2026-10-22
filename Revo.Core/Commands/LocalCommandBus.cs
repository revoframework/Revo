using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Services;

namespace Revo.Core.Commands;

/// <summary>
/// Executes commands locally, each of them in its own service scope.
/// </summary>
/// <remarks>
/// Command handlers, middlewares and all the other services they use (repositories, unit of work etc.) are
/// resolved from a new service scope created for every command (including the nested ones sent from other command
/// handlers) and disposed after the command finishes. Resources that need to outlive the command until the end
/// of the whole request can be registered with <see cref="Lifecycle.IRequestLifetime"/>.
/// </remarks>
public class LocalCommandBus(IServiceScopeFactory serviceScopeFactory) : ILocalCommandBus
{
    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        object result = await ProcessInCommandScopeAsync<TResult>(command, executionOptions, cancellationToken);
        return (TResult) result;
    }

    public Task SendAsync(ICommandBase command, CancellationToken cancellationToken)
    {
        return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
    }

    public async Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken)
    {
        await ProcessInCommandScopeAsync<object>(command, executionOptions, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        return SendAsync<TResult>(command, CommandExecutionOptions.Default, cancellationToken);
    }

    private async Task<object> ProcessInCommandScopeAsync<TResult>(ICommandBase command,
        CommandExecutionOptions executionOptions, CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<ICommandBusPipeline>();
        var serviceResolver = scope.ServiceProvider.GetRequiredService<IVariantServiceResolver>();

        return await pipeline.ProcessAsync(command,
            (processedCommand, ct) => ExecutionHandler<TResult>(serviceResolver, processedCommand, ct),
            this, executionOptions, cancellationToken);
    }

    private async Task<object> ExecutionHandler<TResult>(IVariantServiceResolver serviceResolver,
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