using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Revo.Core.Commands.Configuration;

namespace Revo.Core.Commands;

public static class CommandHandlerBindingExtensions
{
    /// <summary>
    /// Registers command handler class to all of its implemented <see cref="ICommandHandler{T}"/>
    /// and <see cref="IQueryHandler{TQuery,TResult}"/> interfaces. Uses scoped life-time.
    /// Calling this method manually in your modules is not needed when using <see cref="CommandsConfigurationSection.AutoDiscoverCommandHandlers"/>.
    /// </summary>
    /// <typeparam name="T">Command handler type.</typeparam>
    /// <param name="serviceCollection">Service collection.</param>
    /// <param name="additionalServices">Additional services to bind for.</param>
    public static void AddCommandHandler<T>(
        this IServiceCollection serviceCollection, params Type[] additionalServices)
        where T : class
    {
        serviceCollection.AddCommandHandler(typeof(T), additionalServices);
    }

    /// <summary>
    /// Registers command handler class to all of its implemented <see cref="ICommandHandler{T}"/>
    /// and <see cref="IQueryHandler{TQuery,TResult}"/> interfaces. Uses scoped life-time.
    /// Calling this method manually in your modules is not needed when using <see cref="CommandsConfigurationSection.AutoDiscoverCommandHandlers"/>.
    /// </summary>
    /// <param name="serviceCollection">Service collection.</param>
    /// <param name="commandHandlerType">Command handler type.</param>
    /// <param name="additionalServices">Additional services to bind for.</param>
    public static void AddCommandHandler(
        this IServiceCollection serviceCollection, Type commandHandlerType, params Type[] additionalServices)
    {
        if (!commandHandlerType.IsClass || commandHandlerType.IsAbstract)
        {
            throw new ArgumentException($"{commandHandlerType} is not a bindable non-abstract class");
        }

        var handlerInterfaces = GetCommandHandlerInterfaces(commandHandlerType);
        if (handlerInterfaces.Length == 0)
        {
            throw new ArgumentException($"{commandHandlerType} does not implement any ICommandHandler<> nor ICommandHandler<,>");
        }
            
        serviceCollection.TryAddScoped(commandHandlerType);
            
        foreach (var t in handlerInterfaces.Concat(additionalServices))
        {
            serviceCollection.AddTransient(t, sp => sp.GetRequiredService(commandHandlerType));
        }
    }

    public static Type[] GetCommandHandlerInterfaces(Type commandHandlerType)
    {
        return commandHandlerType.GetInterfaces()
            .Where(x => x.IsGenericType
                        && new[] {typeof(ICommandHandler<>), typeof(ICommandHandler<,>)}
                            .Contains(x.GetGenericTypeDefinition()))
            .ToArray();
    }
}