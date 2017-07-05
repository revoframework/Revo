using System;
using System.Linq;
using GTRevo.Core.Commands;
using GTRevo.Core.Events;
using MediatR;
using Ninject;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace GTRevo.Platform.Core
{
    public class CommandBusModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ICommandBus, IEventBus>()
                .To<CommandBus>();

            Kernel.Components.Add<IBindingResolver, ContravariantBindingResolver>();

            Bind<SingleInstanceFactory>().ToMethod(ctx =>
            {
                SingleInstanceFactory fac = t =>
                {
                    if (ctx.Kernel.GetBindings(t).Count() == 0)
                    {
                        return null;
                    }

                    try
                    {
                        var x = ctx.Kernel.Get(t);
                        return x;
                    }
                    catch (Exception e)
                    {
                        throw new NotSupportedException("Error activating command handler: " + e.ToString(), e);
                    }
                };

                return fac;
            });

            Bind<MultiInstanceFactory>().ToMethod(ctx =>
            {
                MultiInstanceFactory fac = t =>
                {
                    return ctx.Kernel.GetAll(t);
                };

                return fac;
            });
        }
    }
}
