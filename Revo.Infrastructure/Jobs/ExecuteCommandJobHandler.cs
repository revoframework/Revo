using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJobHandler(ICommandGateway commandGateway) : IJobHandler<IExecuteCommandJob>
    {
        public Task HandleAsync(IExecuteCommandJob job, CancellationToken cancellationToken)
        {
            return commandGateway.SendAsync(job.Command, cancellationToken);
        }
    }
}
