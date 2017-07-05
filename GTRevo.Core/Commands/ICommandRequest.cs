using System;
using System.Collections.Generic;

namespace GTRevo.Core.Commands
{
    public interface ICommandRequest
    {
        ICommand Command { get; }
        IDictionary<Type, object> Data { get; }
    }
}
