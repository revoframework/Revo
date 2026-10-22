using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;

namespace Revo.Core.Transactions;

public static class UnitOfWorkServiceScopeExtensions
{
    /// <summary>
    /// Executes an action in a new service scope with its own unit of work set as the current command context
    /// (like the command bus does for commands, but without running the command pipeline) and commits the unit
    /// of work if the action succeeds.
    /// </summary>
    /// <remarks>
    /// All services taking part in the unit of work (e.g. repositories, transaction coordinators) need to be
    /// resolved from the service provider passed to the action, so that they all belong to the same scope.
    /// </remarks>
    public static async Task ExecuteInUnitOfWorkScopeAsync(this IServiceScopeFactory serviceScopeFactory,
        Func<IServiceProvider, IUnitOfWork, Task> action)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
        var commandContextStack = scope.ServiceProvider.GetRequiredService<CommandContextStack>();

        using var unitOfWork = unitOfWorkFactory.CreateUnitOfWork();
        commandContextStack.Push(new CommandContext(null, unitOfWork));
        try
        {
            unitOfWork.Begin();
            await action(scope.ServiceProvider, unitOfWork);
            await unitOfWork.CommitAsync();
        }
        finally
        {
            commandContextStack.Pop();
        }
    }
}
