using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Tenancy
{
    public class TenantContextCommandBusMiddleware : ICommandBusMiddleware<ICommandBase>
    {
        public int Order { get; set; } = -3000;

        public async Task<object> HandleAsync(ICommandBase command, CommandExecutionOptions executionOptions,
            CommandBusMiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if (executionOptions.TenantContext != null)
            {
                using (TenantContextOverride.Push(executionOptions.TenantContext.Tenant))
                {
                    return await next(command);
                }
            }
            else
            {
                return await next(command);
            }
        }
    }
}