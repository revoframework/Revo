using System;
using System.Threading;
using System.Threading.Tasks;

namespace Revo.Core.Commands
{
    public class FilterCommandBusMiddleware<T>(Func<IPreCommandFilter<T>[]> preCommandFiltersFunc,
            Func<IPostCommandFilter<T>[]> postCommandFiltersFunc,
            Func<IExceptionCommandFilter<T>[]> exceptionCommandFiltersFunc) : ICommandBusMiddleware<T>
        where T : class, ICommandBase
    {
        public int Order { get; set; } = -1000;

        public async Task<object> HandleAsync(T command, CommandExecutionOptions executionOptions,
            CommandBusMiddlewareDelegate next, CancellationToken cancellationToken)
        {
            await PreFilterAsync(command);

            try
            {
                var result = await next(command);
                await PostFilterAsync(command);
                return result;
            }
            catch (Exception e)
            {
                await FilterExceptionAsync(command, e);
                throw;
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
}