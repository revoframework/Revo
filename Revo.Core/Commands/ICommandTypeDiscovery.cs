using System;
using System.Collections.Generic;

namespace Revo.Core.Commands
{
    public interface ICommandTypeDiscovery
    {
        IEnumerable<Type> DiscoverCommandTypes();
    }
}