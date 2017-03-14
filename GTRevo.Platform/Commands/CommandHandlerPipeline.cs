using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTRevo.Platform.Commands
{
    public class CommandHandlerPipeline<T> : IAsyncCommandHandler<T>
         where T : ICommand
    {
        private readonly IPreCommandHandler<T>[] preCommandHandlers;
        private readonly IAsyncCommandHandler<T> commandHandler;

        public CommandHandlerPipeline(IPreCommandHandler<T>[] preCommandHandlers,
            IAsyncCommandHandler<T> commandHandler)
        {
            this.preCommandHandlers = preCommandHandlers;
            this.commandHandler = commandHandler;
        }

        public async Task Handle(T message)
        {
            await PreHandle(message);
            await commandHandler.Handle(message);
        }

        private async Task PreHandle(T message)
        {
            foreach (var preHandler in preCommandHandlers)
            {
                await preHandler.Handle(message);
            }
        }
    }

    public class CommandHandlerPipeline<T, TResult> : IAsyncCommandHandler<T, TResult>
         where T : ICommand<TResult>
    {
        private readonly IPreCommandHandler<T>[] preCommandHandlers;
        private readonly IAsyncCommandHandler<T, TResult> commandHandler;

        public CommandHandlerPipeline(IPreCommandHandler<T>[] preCommandHandlers,
            IAsyncCommandHandler<T, TResult> commandHandler)
        {
            this.preCommandHandlers = preCommandHandlers;
            this.commandHandler = commandHandler;
        }

        public async Task<TResult> Handle(T message)
        {
            await PreHandle(message);
            return await commandHandler.Handle(message);
        }

        private async Task PreHandle(T message)
        {
            foreach (var preHandler in preCommandHandlers)
            {
                await preHandler.Handle(message);
            }
        }
    }
}
