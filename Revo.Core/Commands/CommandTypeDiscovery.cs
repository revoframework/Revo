using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Types;

namespace Revo.Core.Commands
{
    public class CommandTypeDiscovery : ICommandTypeDiscovery
    {
        private readonly ITypeExplorer typeExplorer;

        public CommandTypeDiscovery(ITypeExplorer typeExplorer)
        {
            this.typeExplorer = typeExplorer;
        }

        public IEnumerable<Type> DiscoverCommandTypes()
        {
            return typeExplorer
                .GetAllTypes()
                .Where(x => x.IsClass && !x.IsAbstract)
                .Where(x => typeof(ICommandBase).IsAssignableFrom(x));
        }
    }
}