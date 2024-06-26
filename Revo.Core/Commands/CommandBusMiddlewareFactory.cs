using System.Linq;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public class CommandBusMiddlewareFactory(IServiceLocator serviceLocator) : ICommandBusMiddlewareFactory
    {
        private readonly IServiceLocator serviceLocator = serviceLocator;

        public ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
            where TCommand : class, ICommandBase => 
            serviceLocator.GetAll<ICommandBusMiddleware<TCommand>>()
                          .ToArray();
    }
}