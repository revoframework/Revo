using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Commands;

public class CommandBusMiddlewareFactory(IServiceProvider serviceProvider) : ICommandBusMiddlewareFactory
{
    public ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
        where TCommand : class, ICommandBase
    {
        return serviceProvider.GetServices<ICommandBusMiddleware<TCommand>>()
            .ToArray();
    }
}