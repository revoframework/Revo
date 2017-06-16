using System;
using System.Collections.Generic;

namespace GTRevo.Commands
{
    public interface ICommandRequest
    {
        ICommand Command { get; }
        IDictionary<Type, object> Data { get; }
    }
}
