namespace Revo.Core.Commands
{
    public interface ICommandBusMiddlewareFactory
    {
        ICommandBusMiddleware<TCommand>[] CreateMiddlewares<TCommand>(ICommandBus commandBus)
            where TCommand : class, ICommandBase;
    }
}