using System;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Core;

namespace Revo.Core.Commands
{
    public class CommandHandlerPipeline<T> : ICommandHandler<T>
         where T : ICommand
    {
        private readonly Func<IPreCommandFilter<T>[]> preCommandFiltersFunc;
        private readonly Func<IPostCommandFilter<T>[]> postCommandFiltersFunc;
        private readonly Func<IExceptionCommandFilter<T>[]> exceptionCommandFiltersFunc;
        private readonly Func<ICommandHandler<T>> commandHandlerFunc;

        public CommandHandlerPipeline(Func<IPreCommandFilter<T>[]> preCommandFiltersFunc,
            Func<IPostCommandFilter<T>[]> postCommandFiltersFunc,
            Func<IExceptionCommandFilter<T>[]> exceptionCommandFiltersFunc,
            Func<ICommandHandler<T>> commandHandlerFunc)
        {
            this.preCommandFiltersFunc = preCommandFiltersFunc;
            this.postCommandFiltersFunc = postCommandFiltersFunc;
            this.exceptionCommandFiltersFunc = exceptionCommandFiltersFunc;
            this.commandHandlerFunc = commandHandlerFunc;
        }

        public async Task HandleAsync(T message, CancellationToken cancellationToken)
        {
            using (TaskContext.Enter())
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
        }

        private async Task PreFilterAsync(T message)
        {
            var preCommandFilters = preCommandFiltersFunc();
            foreach (var filter in preCommandFilters)
            {
                await filter.PreFilterAsync(message);
            }
        }

        private async Task PostFilterAsync(T message)
        {
            var postCommandFilters = postCommandFiltersFunc();
            foreach (var filter in postCommandFilters)
            {
                await filter.PostFilterAsync(message, null);
            }
        }

        private async Task FilterExceptionAsync(T message, Exception e)
        {
            var exceptionCommandFilters = exceptionCommandFiltersFunc();
            foreach (var filter in exceptionCommandFilters)
            {
                await filter.FilterExceptionAsync(message, e);
            }
        }
    }

    public class CommandHandlerPipeline<T, TResult> : ICommandHandler<T, TResult>
         where T : ICommand<TResult>
    {
        private readonly Func<IPreCommandFilter<T>[]> preCommandFiltersFunc;
        private readonly Func<IPostCommandFilter<T>[]> postCommandFiltersFunc;
        private readonly Func<IExceptionCommandFilter<T>[]> exceptionCommandFiltersFunc;
        private readonly Func<ICommandHandler<T, TResult>> commandHandlerFunc;

        public CommandHandlerPipeline(Func<IPreCommandFilter<T>[]> preCommandFiltersFunc,
            Func<IPostCommandFilter<T>[]> postCommandFiltersFunc,
            Func<IExceptionCommandFilter<T>[]> exceptionCommandFiltersFunc,
            Func<ICommandHandler<T, TResult>> commandHandlerFunc)
        {
            this.preCommandFiltersFunc = preCommandFiltersFunc;
            this.postCommandFiltersFunc = postCommandFiltersFunc;
            this.exceptionCommandFiltersFunc = exceptionCommandFiltersFunc;
            this.commandHandlerFunc = commandHandlerFunc;
        }

        public async Task<TResult> HandleAsync(T message, CancellationToken cancellationToken)
        {
            using (TaskContext.Enter())
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
        }

        private async Task PreFilterAsync(T message)
        {
            var preCommandFilters = preCommandFiltersFunc();
            foreach (var filter in preCommandFilters)
            {
                await filter.PreFilterAsync(message);
            }
        }

        private async Task PostFilterAsync(T message, object result)
        {
            var postCommandFilters = postCommandFiltersFunc();
            foreach (var filter in postCommandFilters)
            {
                await filter.PostFilterAsync(message, result);
            }
        }

        private async Task FilterExceptionAsync(T message, Exception e)
        {
            var exceptionCommandFilters = exceptionCommandFiltersFunc();
            foreach (var filter in exceptionCommandFilters)
            {
                await filter.FilterExceptionAsync(message, e);
            }
        }
    }
}
