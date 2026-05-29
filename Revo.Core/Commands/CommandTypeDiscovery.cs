using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Types;

namespace Revo.Core.Commands
{
    public class CommandTypeDiscovery(ITypeExplorer typeExplorer) : ICommandTypeDiscovery
    {
        private readonly ITypeExplorer typeExplorer = typeExplorer;

        public IEnumerable<Type> DiscoverCommandTypes() =>
            typeExplorer
                .GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract)
                .Where(x => typeof(ICommandBase).IsAssignableFrom(x));
    }
}