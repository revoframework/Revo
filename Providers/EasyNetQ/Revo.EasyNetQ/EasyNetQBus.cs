using System;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Logging;
using Revo.Core.Events;
using Revo.Infrastructure.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQBus(IBus bus,
        IEventMessageFactory eventMessageFactory,
        ILogger<EasyNetQBus> logger)
        : IEasyNetQBus
    {
        public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var message = (IEventMessage<TEvent>) await eventMessageFactory.CreateMessageAsync(@event);
            await PublishAsync(message);
        }

        public async Task PublishAsync<TEvent>(TEvent @event, string topic) where TEvent : IEvent
        {
            var message = (IEventMessage<TEvent>)await eventMessageFactory.CreateMessageAsync(@event);
            await PublishAsync(message, topic);
        }

        public async Task PublishAsync<TEvent>(TEvent @event, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent
        {
            var message = (IEventMessage<TEvent>)await eventMessageFactory.CreateMessageAsync(@event);
            await PublishAsync(message, publishConfiguration);
        }

        public async Task PublishAsync<TEvent>(IEventMessage<TEvent> message) where TEvent : IEvent
        {
            if (!message.GetType().IsConstructedGenericType
                || message.GetType().GetGenericTypeDefinition() != typeof(EventMessage<>))
            {
                message = (IEventMessage<TEvent>) EventMessage.FromEvent(message.Event, message.Metadata);
            }
            
            logger.LogDebug("Publishing event {EventName}", message.Event.GetType().Name);
            await bus.PubSub.PublishAsync(message);
        }

        public async Task PublishAsync<TEvent>(IEventMessage<TEvent> message, string topic) where TEvent : IEvent
        {
            if (!message.GetType().IsConstructedGenericType
                || message.GetType().GetGenericTypeDefinition() != typeof(EventMessage<>))
            {
                message = (IEventMessage<TEvent>) EventMessage.FromEvent(message.Event, message.Metadata);
            }
            
            logger.LogDebug("Publishing event {EventName} in topic {TOpic}", message.Event.GetType().Name, topic);
            await bus.PubSub.PublishAsync(message, topic);
        }

        public async Task PublishAsync<TEvent>(IEventMessage<TEvent> message, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent
        {
            if (!message.GetType().IsConstructedGenericType
                || message.GetType().GetGenericTypeDefinition() != typeof(EventMessage<>))
            {
                message = (IEventMessage<TEvent>) EventMessage.FromEvent(message.Event, message.Metadata);
            }
            
            logger.LogDebug("Publishing event {EventName} with custom configuration", message.Event.GetType().Name);
            await bus.PubSub.PublishAsync(message, publishConfiguration);
        }
    }
}
