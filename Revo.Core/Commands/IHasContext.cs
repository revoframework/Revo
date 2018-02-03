using System;

namespace Revo.Core.Commands
{
    public interface IHasContext : ICommandBase
    {
        Guid[] Context { get; set; }
    }
}
