using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJobHandler : IJobHandler<IExecuteCommandJob>
    {
        private readonly ICommandGateway commandGateway;

        public ExecuteCommandJobHandler(ICommandGateway commandGateway)
        {
            this.commandGateway = commandGateway;
        }

        public Task HandleAsync(IExecuteCommandJob job, CancellationToken cancellationToken)
        {
            return commandGateway.SendAsync(job.Command, cancellationToken);
        }
    }
}
