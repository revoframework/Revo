using System;
using System.Threading.Tasks;
using EasyNetQ;
using Revo.Core.Events;
using Revo.Infrastructure.Events;

namespace Revo.EasyNetQ
{
    public class EasyNetQBus : IEasyNetQBus
    {
        private readonly IBus bus;
        private readonly IEventMessageFactory eventMessageFactory;

        public EasyNetQBus(IBus bus, IEventMessageFactory eventMessageFactory)
        {
            this.bus = bus;
            this.eventMessageFactory = eventMessageFactory;
        }

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
            
            await bus.PubSub.PublishAsync(message);
        }

        public async Task PublishAsync<TEvent>(IEventMessage<TEvent> message, string topic) where TEvent : IEvent
        {
            if (!message.GetType().IsConstructedGenericType
                || message.GetType().GetGenericTypeDefinition() != typeof(EventMessage<>))
            {
                message = (IEventMessage<TEvent>) EventMessage.FromEvent(message.Event, message.Metadata);
            }
            
            await bus.PubSub.PublishAsync(message, topic);
        }

        public async Task PublishAsync<TEvent>(IEventMessage<TEvent> message, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent
        {
            if (!message.GetType().IsConstructedGenericType
                || message.GetType().GetGenericTypeDefinition() != typeof(EventMessage<>))
            {
                message = (IEventMessage<TEvent>) EventMessage.FromEvent(message.Event, message.Metadata);
            }
            
            await bus.PubSub.PublishAsync(message, publishConfiguration);
        }
    }
}
