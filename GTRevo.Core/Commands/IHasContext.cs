using System;

namespace GTRevo.Core.Commands
{
    public interface IHasContext : ICommandBase
    {
        Guid[] Context { get; set; }
    }
}
