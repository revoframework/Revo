using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Infrastructure.Events.Async;

public static class AsyncEventListenerBindingExtensions
{
    /// <summary>
    /// Binds event listener class and its event sequencer class to all of its implemented <see cref="IAsyncEventListener{T}"/>
    /// and <see cref="IAsyncEventSequencer{TEvent}"/> interfaces correspondingly. Both are scoped (one instance
    /// per scope, e.g. per async event worker run), shared by all of their interfaces.
    /// </summary>
    /// <typeparam name="TListener">Async event listener class that implements <see cref="IAsyncEventListener{T}"/>.</typeparam>
    /// <typeparam name="TSequencer">Async event sequencer class that implements <see cref="IAsyncEventSequencer{T}"/>.</typeparam>
    /// <param name="serviceCollection">Service collection to register the listener to.</param>
    /// <param name="additionalListenerServices">Additional services to bind the listener for.</param>
    public static IServiceCollection BindAsyncEventListener<TListener, TSequencer>(
        this IServiceCollection serviceCollection, params Type[] additionalListenerServices)
        where TListener : class, IAsyncEventListener
        where TSequencer : class, IAsyncEventSequencer
    {
        if (!typeof(TListener).IsClass || typeof(TListener).IsAbstract)
        {
            throw new ArgumentException($"{typeof(TListener)} is not a bindable non-abstract class");
        }

        if (!typeof(TSequencer).IsClass || typeof(TSequencer).IsAbstract)
        {
            throw new ArgumentException($"{typeof(TSequencer)} is not a bindable non-abstract class");
        }

        var sequencerInterfaces = GetAsyncSequencerInterfaces(typeof(TSequencer));
        if (sequencerInterfaces.Length == 0)
        {
            throw new ArgumentException($"{typeof(TSequencer)} does not implement any IAsyncEventSequencer<>");
        }

        var listenerInterfaces = GetAsyncListenerInterfaces(typeof(TListener));
        if (sequencerInterfaces.Length == 0)
        {
            throw new ArgumentException($"{typeof(TListener)} does not implement any IAsyncEventListener<>");
        }

        serviceCollection.AddScoped<TSequencer>();
        foreach (var sequencerInterface in sequencerInterfaces)
        {
            serviceCollection.AddTransient(sequencerInterface,
                serviceProvider => serviceProvider.GetRequiredService<TSequencer>());
        }
        
        serviceCollection.AddScoped<TListener>();
        foreach (var listenerInterface in listenerInterfaces.Concat(additionalListenerServices))
        {
            serviceCollection.AddTransient(listenerInterface,
                serviceProvider => serviceProvider.GetRequiredService<TListener>());
        }

        return serviceCollection;
    }

    /// <summary>
    /// Binds event listener class and its event sequencer class to all of its implemented <see cref="IAsyncEventListener{T}"/>
    /// and <see cref="IAsyncEventSequencer{TEvent}"/> interfaces correspondingly. Both are scoped (one instance
    /// per scope, e.g. per async event worker run), shared by all of their interfaces.
    /// </summary>
    /// <param name="serviceCollection">Service collection to register the listener to.</param>
    /// <param name="listenerType">Async event listener class that implements <see cref="IAsyncEventListener{T}"/>.</param>
    /// <param name="sequencerType">Async event sequencer class that implements <see cref="IAsyncEventSequencer{T}"/>.</param>
    /// <param name="additionalListenerServices">Additional services to bind the listener for.</param>
    public static IServiceCollection BindAsyncEventListener(
        this IServiceCollection serviceCollection, Type listenerType, Type sequencerType, params Type[] additionalListenerServices)
    {
        if (!listenerType.IsClass || listenerType.IsAbstract)
        {
            throw new ArgumentException($"{listenerType} is not a bindable non-abstract class");
        }

        if (!sequencerType.IsClass || sequencerType.IsAbstract)
        {
            throw new ArgumentException($"{sequencerType} is not a bindable non-abstract class");
        }

        var sequencerInterfaces = GetAsyncSequencerInterfaces(sequencerType);
        if (sequencerInterfaces.Length == 0)
        {
            throw new ArgumentException($"{sequencerType} does not implement any IAsyncEventSequencer<>");
        }

        var listenerInterfaces = GetAsyncListenerInterfaces(listenerType);
        if (sequencerInterfaces.Length == 0)
        {
            throw new ArgumentException($"{listenerType} does not implement any IAsyncEventListener<>");
        }

        serviceCollection.AddScoped(sequencerType);
        foreach (var sequencerInterface in sequencerInterfaces)
        {
            serviceCollection.AddTransient(sequencerInterface,
                serviceProvider => serviceProvider.GetRequiredService(sequencerType));
        }

        serviceCollection.AddScoped(listenerType);
        foreach (var listenerInterface in listenerInterfaces.Concat(additionalListenerServices))
        {
            serviceCollection.AddTransient(listenerInterface,
                serviceProvider => serviceProvider.GetRequiredService(listenerType));
        }
        
        return serviceCollection;
    }

    public static Type[] GetAsyncListenerInterfaces(Type commandHandlerType)
    {
        return commandHandlerType.GetInterfaces()
            .Where(x => x.IsGenericType
                        && typeof(IAsyncEventListener<>) == x.GetGenericTypeDefinition())
            .ToArray();
    }

    public static Type[] GetAsyncSequencerInterfaces(Type commandHandlerType)
    {
        return commandHandlerType.GetInterfaces()
            .Where(x => x.IsGenericType
                        && typeof(IAsyncEventSequencer<>) == x.GetGenericTypeDefinition())
            .ToArray();
    }
}