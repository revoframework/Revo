using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Revo.Core.Events;
using Revo.EasyNetQ.Configuration;

namespace Revo.EasyNetQ;

public class BusInitializer : IHostedLifecycleService
{
    private static readonly MethodInfo SubscribeBlockingMethod = typeof(BusInitializer)
        .GetMethod(nameof(SubscribeBlocking), BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MethodInfo SubscribeMethod = typeof(BusInitializer)
        .GetMethod(nameof(Subscribe), BindingFlags.Instance | BindingFlags.NonPublic);

    private readonly IBus bus;
    private readonly IEasyNetQSubscriptionHandler subscriptionHandler;
    private readonly IEasyNetQBlockingSubscriptionHandler blockingSubscriptionHandler;
    private readonly EasyNetQConfigurationSection configurationSection;

    public BusInitializer(IBus bus, IEasyNetQSubscriptionHandler subscriptionHandler,
        IEasyNetQBlockingSubscriptionHandler blockingSubscriptionHandler,
        EasyNetQConfigurationSection configurationSection)
    {
        this.bus = bus;
        this.subscriptionHandler = subscriptionHandler;
        this.blockingSubscriptionHandler = blockingSubscriptionHandler;
        this.configurationSection = configurationSection;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StartedAsync(CancellationToken cancellationToken)
    {
        foreach (var eventType in configurationSection.Subscriptions.Events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (eventType.Value.IsBlockingSubscriber)
            {
                var subscribeTask = (Task)SubscribeBlockingMethod.MakeGenericMethod(eventType.Key)
                    .Invoke(this, new object[] { eventType.Value });
                await subscribeTask.ConfigureAwait(false);
            }
            else
            {
                var subscribeTask = (Task)SubscribeMethod.MakeGenericMethod(eventType.Key)
                    .Invoke(this, new object[] { eventType.Value });
                await subscribeTask.ConfigureAwait(false);
            }
        }
    }

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SubscribeBlocking<T>(EasyNetQSubscriptionsConfiguration.SubscriptionConfiguration subscriptionConfiguration)
        where T : class, IEvent
    {
        Func<IEventMessage<T>, CancellationToken, Task> onMessage = (e, c) =>
        {
            blockingSubscriptionHandler.HandleMessage(e);
            return Task.CompletedTask;
        };

        await bus.PubSub.SubscribeAsync(subscriptionConfiguration.SubscriptionId,
            onMessage,
            subscriptionConfiguration.ConfigurationAction ?? (_ => { }));
    }

    private async Task Subscribe<T>(EasyNetQSubscriptionsConfiguration.SubscriptionConfiguration subscriptionConfiguration)
        where T : class, IEvent
    {
        await bus.PubSub.SubscribeAsync<IEventMessage<T>>(subscriptionConfiguration.SubscriptionId,
            (e, c) => subscriptionHandler.HandleMessageAsync(e),
            subscriptionConfiguration.ConfigurationAction ?? (_ => { }));
    }
}
