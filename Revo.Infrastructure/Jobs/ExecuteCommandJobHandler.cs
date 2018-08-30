using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJobHandler : IJobHandler<IExecuteCommandJob>
    {
        private readonly ICommandBus commandBus;

        public ExecuteCommandJobHandler(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        public Task HandleAsync(IExecuteCommandJob job, CancellationToken cancellationToken)
        {
            return commandBus.SendAsync(job.Command, cancellationToken);
        }
    }
}
