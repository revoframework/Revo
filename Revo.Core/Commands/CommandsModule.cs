using Ninject.Modules;
using Revo.Core.Core;
using Revo.Core.Lifecycle;
using Revo.Core.Transactions;

namespace Revo.Core.Commands
{
    [AutoLoadModule(false)]
    public class CommandsModule : NinjectModule
    {
        private readonly CommandsConfiguration configurationSection;

        public CommandsModule(CommandsConfiguration configurationSection)
        {
            this.configurationSection = configurationSection;
        }

        public override void Load()
        {
            Bind<ICommandGateway>()
                .To<CommandGateway>()
                .InSingletonScope();

            Bind<ICommandBus, ILocalCommandBus>()
                .To<LocalCommandBus>()
                .InSingletonScope();

            Bind<ICommandBusMiddlewareFactory>()
                .To<CommandBusMiddlewareFactory>()
                .InSingletonScope();

            Bind<ICommandBusPipeline>()
                .To<CommandBusPipeline>()
                .InTransientScope();

            Bind(typeof(ICommandBusMiddleware<>))
                .To(typeof(FilterCommandBusMiddleware<>))
                .InTransientScope();

            Bind<ICommandBusMiddleware<ICommandBase>>()
                .To<UnitOfWorkCommandBusMiddleware>()
                .InTransientScope();

            Bind<ICommandTypeDiscovery>()
                .To<CommandTypeDiscovery>();

            if (configurationSection.AutoDiscoverCommandHandlers)
            {
                Bind<IApplicationConfigurer>()
                    .To<CommandHandlerDiscovery>()
                    .InSingletonScope();
            }

            Bind<ICommandRouter>()
                .To<CommandRouter>()
                .InSingletonScope()
                .OnActivation(commandRouter =>
                {
                    foreach (var route in configurationSection.CommandRoutes)
                    {
                        commandRouter.AddRoute(route.Key, route.Value);
                    }
                });

            Bind<ICommandContext, IUnitOfWorkAccessor, CommandContextStack>()
                .To<CommandContextStack>()
                .InTaskScope();
        }
    }
}
