using System;
using Revo.Core.Commands;

namespace Revo.Infrastructure.Jobs
{
    public interface IEnqueueJobCommand : ICommand
    {
        ICommandBase Command { get; }
        TimeSpan? TimeDelay { get; }
    }
}
