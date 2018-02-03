using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJob : IJob
    {
        public ExecuteCommandJob(ICommandBase command)
        {
            Command = command;
        }

        public ICommandBase Command { get; }
        //public Guid ExecutionParameters...
        //tenant context
    }
}
