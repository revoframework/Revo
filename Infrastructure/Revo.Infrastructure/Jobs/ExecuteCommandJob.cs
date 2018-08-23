using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public class ExecuteCommandJob<T> : IExecuteCommandJob
        where T : ICommandBase
    {
        public ExecuteCommandJob(T command)
        {
            Command = command;
        }

        public T Command { get; }

        //public Guid ExecutionParameters...
        //tenant context

        ICommandBase IExecuteCommandJob.Command => Command;
    }
}
