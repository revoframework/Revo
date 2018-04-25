using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public class CommandHandlerPipeline<T> : ICommandHandler<T>
         where T : ICommand
    {
        private readonly IPreCommandFilter<T>[] preCommandFilters;
        private readonly IPostCommandFilter<T>[] postCommandFilters;
        private readonly IExceptionCommandFilter<T>[] exceptionCommandFilters;
        private readonly Func<ICommandHandler<T>> commandHandlerFunc;

        public CommandHandlerPipeline(IPreCommandFilter<T>[] preCommandFilters,
            IPostCommandFilter<T>[] postCommandFilters,
            IExceptionCommandFilter<T>[] exceptionCommandFilters,
            Func<ICommandHandler<T>> commandHandlerFunc)
        {
            this.preCommandFilters = preCommandFilters;
            this.postCommandFilters = postCommandFilters;
            this.exceptionCommandFilters = exceptionCommandFilters;
            this.commandHandlerFunc = commandHandlerFunc;
        }

        public async Task HandleAsync(T message, CancellationToken cancellationToken)
        {
            await PreFilterAsync(message);
            
            try
            {
                var commandHandler = commandHandlerFunc();
                await commandHandler.HandleAsync(message, cancellationToken);
            }
            catch (Exception e)
            {
                await FilterExceptionAsync(message, e);
                throw;
            }

            await PostFilterAsync(message);
        }

        private async Task PreFilterAsync(T message)
        {
            foreach (var filter in preCommandFilters)
            {
                await filter.PreFilterAsync(message);
            }
        }

        private async Task PostFilterAsync(T message)
        {
            foreach (var filter in postCommandFilters)
            {
                await filter.PostFilterAsync(message, null);
            }
        }

        private async Task FilterExceptionAsync(T message, Exception e)
        {
            foreach (var filter in exceptionCommandFilters)
            {
                await filter.FilterExceptionAsync(message, e);
            }
        }
    }

    public class CommandHandlerPipeline<T, TResult> : ICommandHandler<T, TResult>
         where T : ICommand<TResult>
    {
        private readonly IPreCommandFilter<T>[] preCommandFilters;
        private readonly IPostCommandFilter<T>[] postCommandFilters;
        private readonly IExceptionCommandFilter<T>[] exceptionCommandFilters;
        private readonly Func<ICommandHandler<T, TResult>> commandHandlerFunc;

        public CommandHandlerPipeline(IPreCommandFilter<T>[] preCommandFilters,
            IPostCommandFilter<T>[] postCommandFilters,
            IExceptionCommandFilter<T>[] exceptionCommandFilters,
            Func<ICommandHandler<T, TResult>> commandHandlerFunc)
        {
            this.preCommandFilters = preCommandFilters;
            this.postCommandFilters = postCommandFilters;
            this.exceptionCommandFilters = exceptionCommandFilters;
            this.commandHandlerFunc = commandHandlerFunc;
        }

        public async Task<TResult> HandleAsync(T message, CancellationToken cancellationToken)
        {
            await PreFilterAsync(message);

            TResult result;
            try
            {
                var commandHandler = commandHandlerFunc();
                result = await commandHandler.HandleAsync(message, cancellationToken);
            }
            catch (Exception e)
            {
                await FilterExceptionAsync(message, e);
                throw;
            }

            await PostFilterAsync(message, result);
            return result;
        }

        private async Task PreFilterAsync(T message)
        {
            foreach (var filter in preCommandFilters)
            {
                await filter.PreFilterAsync(message);
            }
        }

        private async Task PostFilterAsync(T message, object result)
        {
            foreach (var filter in postCommandFilters)
            {
                await filter.PostFilterAsync(message, result);
            }
        }

        private async Task FilterExceptionAsync(T message, Exception e)
        {
            foreach (var filter in exceptionCommandFilters)
            {
                await filter.FilterExceptionAsync(message, e);
            }
        }
    }
}
