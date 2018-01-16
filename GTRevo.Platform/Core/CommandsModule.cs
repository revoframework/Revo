using System;
using System.Linq;
using GTRevo.Core.Commands;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace GTRevo.Platform.Core
{
    public class CommandBusModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommandBus>()
                .To<CommandBus>();

            Kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();

            Bind<IPreCommandFilter<ICommandBase>, IPostCommandFilter<ICommandBase>,
                    IExceptionCommandFilter<ICommandBase>>()
                .To<UnitOfWorkCommandFilter>();
        }
    }
}
