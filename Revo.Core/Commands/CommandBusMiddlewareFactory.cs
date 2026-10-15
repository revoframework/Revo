using System.Linq;
using Revo.Core.Services;

namespace Revo.Core.Commands;

public class CommandBusMiddlewareFactory(IVariantServiceResolver serviceResolver) : ICommandBusMiddlewareFactory
{
    public ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
        where TCommand : class, ICommandBase
    {
        // variant resolution, so that e.g. ICommandBusMiddleware<ICommandBase> applies to all commands
        return serviceResolver.GetServices<ICommandBusMiddleware<TCommand>>()
            .ToArray();
    }
}
