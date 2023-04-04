using System;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.Core.Transactions;

namespace Revo.Core.Commands.Configuration;

public static class CommandConfigurationExtensions
{
    public static IRevoConfiguration ConfigureCommands(this IRevoConfiguration configuration,
        Action<CommandsConfigurationSection> advancedAction = null)
    {
        configuration.ConfigureCore();

        var section = configuration.GetOrAddSection<CommandsConfigurationSection>(cfg =>
        {
            var commandsSection = new CommandsConfigurationSection(cfg);

            cfg.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton(commandsSection);
                serviceCollection.AddScoped<ICommandGateway, CommandGateway>();

                serviceCollection.AddScoped<LocalCommandBus>();
                serviceCollection.AddTransient<ICommandBus>(serviceProvider =>
                    serviceProvider.GetRequiredService<LocalCommandBus>());
                serviceCollection.AddTransient<ILocalCommandBus>(serviceProvider =>
                    serviceProvider.GetRequiredService<LocalCommandBus>());

                serviceCollection.AddScoped<ICommandBusMiddlewareFactory, CommandBusMiddlewareFactory>();
                serviceCollection.AddTransient<ICommandBusPipeline, CommandBusPipeline>();
                serviceCollection.AddTransient(typeof(ICommandBusMiddleware<>),
                    typeof(FilterCommandBusMiddleware<>));
                serviceCollection.AddSingleton<ICommandTypeDiscovery, CommandTypeDiscovery>();
                serviceCollection.AddSingleton<ICommandRouter, CommandRouter>();

                serviceCollection.AddScoped<CommandContextStack>();
                serviceCollection.AddTransient<ICommandContext>(serviceProvider =>
                    serviceProvider.GetRequiredService<CommandContextStack>());
                serviceCollection.AddTransient<IUnitOfWorkAccessor>(serviceProvider =>
                    serviceProvider.GetRequiredService<CommandContextStack>());
            });

            cfg.AddAfterConfigureServicesAction(serviceCollection =>
            {
                if (commandsSection.AutoDiscoverCommandHandlers)
                {
                    DiscoverCommandHandlers(cfg, serviceCollection);
                }
            });

            return commandsSection;
        });
            
        advancedAction?.Invoke(section);

        return configuration;
    }

    private static void DiscoverCommandHandlers(IRevoConfiguration configuration,
        IServiceCollection serviceCollection)
    {
        var commandHandlerDiscovery = new CommandHandlerDiscovery(configuration);
        commandHandlerDiscovery.DiscoverCommandHandlers(serviceCollection);
    }
}