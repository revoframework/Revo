using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Revo.Core.Commands;

namespace Revo.Core.Core
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
