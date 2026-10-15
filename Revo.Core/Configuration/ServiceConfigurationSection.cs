using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Core.Configuration;

public class ServiceConfigurationSection : IRevoConfigurationSection
{
    private readonly List<Action<IServiceCollection>> configureServicesActions = new();
    private readonly List<Action<IServiceCollection>> afterConfigureServicesActions = new();

    public IReadOnlyCollection<Action<IServiceCollection>> ConfigureServicesActions => configureServicesActions;
    public IReadOnlyCollection<Action<IServiceCollection>> AfterConfigureServicesActions => afterConfigureServicesActions;

    public void AddConfigureServicesAction(Action<IServiceCollection> action)
    {
        configureServicesActions.Add(action);
    }

    public void AddAfterConfigureServicesAction(Action<IServiceCollection> action)
    {
        afterConfigureServicesActions.Add(action);
    }
}
