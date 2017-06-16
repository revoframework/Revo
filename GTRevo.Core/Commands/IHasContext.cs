using System;

namespace GTRevo.Commands
{
    public interface IHasContext : ICommandBase
    {
        Guid[] Context { get; set; }
    }
}
