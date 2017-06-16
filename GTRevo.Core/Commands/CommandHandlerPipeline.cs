using System.Threading.Tasks;

namespace GTRevo.Commands
{
    public class CommandHandlerPipeline<T> : IAsyncCommandHandler<T>
         where T : ICommand
    {
        private readonly ICommandFilter<T>[] commandFilters;
        private readonly IAsyncCommandHandler<T> commandHandler;

        public CommandHandlerPipeline(ICommandFilter<T>[] commandFilters,
            IAsyncCommandHandler<T> commandHandler)
        {
            this.commandFilters = commandFilters;
            this.commandHandler = commandHandler;
        }

        public async Task Handle(T message)
        {
            await Filter(message);
            await commandHandler.Handle(message);
        }

        private async Task Filter(T message)
        {
            foreach (var filter in commandFilters)
            {
                await filter.Handle(message);
            }
        }
    }

    public class CommandHandlerPipeline<T, TResult> : IAsyncCommandHandler<T, TResult>
         where T : ICommand<TResult>
    {
        private readonly ICommandFilter<T>[] commandFilters;
        private readonly IAsyncCommandHandler<T, TResult> commandHandler;

        public CommandHandlerPipeline(ICommandFilter<T>[] commandFilters,
            IAsyncCommandHandler<T, TResult> commandHandler)
        {
            this.commandFilters = commandFilters;
            this.commandHandler = commandHandler;
        }

        public async Task<TResult> Handle(T message)
        {
            await Filter(message);
            return await commandHandler.Handle(message);
        }

        private async Task Filter(T message)
        {
            foreach (var filter in commandFilters)
            {
                await filter.Handle(message);
            }
        }
    }
}
