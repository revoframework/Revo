using System;

namespace GTRevo.Platform.Commands
{
    public interface IQueryWithContext<out T> : ICommandWithContext<T>, IQuery<T>
    {
    }
}
