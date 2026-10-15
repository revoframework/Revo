using System;
using Revo.Core.Commands.Configuration;

namespace Revo.Core.Commands;

/// <summary>Defines which command types get routed to which command bus.</summary>
/// <remarks>When using <see cref="CommandsConfigurationSection.AutoDiscoverCommandHandlers"/>, local command handlers
/// get their routes registered automatically.</remarks>
/// <example>
/// Routes can be configured upon framework startup like this:
/// <code>
/// services.AddRevo(revo =&gt; revo
///   .ConfigureCommands(commands =&gt; commands
///     .AddCommandRoute&lt;MyCommand&gt;(serviceProvider =&gt; serviceProvider.GetRequiredService&lt;ICustomCommandBus&gt;())));
/// </code>
/// </example>
public interface ICommandRouter
{
    void AddRoute(Type commandType, Func<IServiceProvider, ICommandBus> commandBusFunc);
    Func<IServiceProvider, ICommandBus> FindRoute(Type commandType);
    bool HasRoute(Type commandType);
    void RemoveRoute(Type commandType);
}