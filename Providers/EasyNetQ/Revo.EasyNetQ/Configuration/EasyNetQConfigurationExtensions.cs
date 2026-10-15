using System;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Configuration;
using Revo.Core.Events;

namespace Revo.EasyNetQ.Configuration;

public static class EasyNetQConfigurationExtensions
{
    public static IRevoConfiguration UseEasyNetQ(this IRevoConfiguration configuration,
        bool? isActive = true,
        EasyNetQConnectionConfiguration connection = null,
        Action<EasyNetQSubscriptionsConfiguration> subscriptions = null,
        Action<EasyNetQConfigurationSection> advancedAction = null)
    {
        var section = configuration.GetOrAddSection<EasyNetQConfigurationSection>(cfg =>
        {
            var s = new EasyNetQConfigurationSection();
            cfg.ConfigureServices(services =>
            {
                services.AddSingleton(s);

                if (s.IsActive)
                {
                    services.RegisterEasyNetQ(s.Connection.ConnectionString,
                        serviceRegister =>
                        {
                            s.RegisterServices?.Invoke(serviceRegister);
                        });

                    services.AddSingleton<BusInitializer>();
                    services.AddHostedService(sp => sp.GetRequiredService<BusInitializer>());

                    services.AddScoped<IEasyNetQBus, EasyNetQBus>();

                    services.AddSingleton<IEasyNetQBlockingSubscriptionHandler, EasyNetQBlockingSubscriptionHandler>();
                    services.AddSingleton<IEasyNetQSubscriptionHandler, EasyNetQSubscriptionHandler>();

                    foreach (var eventTransportPair in s.EventTransports.Events)
                    {
                        var listenerType = typeof(IEventListener<>).MakeGenericType(eventTransportPair.Key);
                        var implementationType = typeof(EventTransport<,>).MakeGenericType(eventTransportPair.Key, eventTransportPair.Value);
                        services.AddScoped(listenerType, implementationType);
                    }
                }
                else
                {
                    services.AddSingleton<IEasyNetQBus, NullEasyNetQBus>();
                }
            });
            return s;
        });
        section.IsActive = isActive ?? section.IsActive;
        section.Connection = connection ?? section.Connection;

        subscriptions?.Invoke(section.Subscriptions);
        advancedAction?.Invoke(section);

        return configuration;
    }
}
