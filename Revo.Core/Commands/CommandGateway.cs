using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands;

public class CommandGateway(ICommandRouter commandRouter,
    IServiceProvider serviceProvider)
    : ICommandGateway
{
    public Task SendAsync(ICommandBase command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        var route = commandRouter.FindRoute(command.GetType())
                    ?? throw new ArgumentException($"No route to a command bus found for command type {command.GetType()}");
        var commandBus = route(serviceProvider);
        return commandBus.SendAsync(command, executionOptions, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
    }

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CommandExecutionOptions executionOptions,
        CancellationToken cancellationToken = default)
    {
        var route = commandRouter.FindRoute(command.GetType())
                    ?? throw new ArgumentException($"No route to a command bus found for command type {command.GetType()}");
            
        var commandBus = route(serviceProvider);
        return commandBus.SendAsync(command, executionOptions, cancellationToken);
    }

    public Task SendAsync(ICommandBase command, CancellationToken cancellationToken = default)
    {
        return SendAsync(command, CommandExecutionOptions.Default, cancellationToken);
    }
}