using System;
using System.Threading.Tasks;
using EasyNetQ;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public class NullEasyNetQBus : IEasyNetQBus
    {
        public Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(TEvent @event, string topic) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(TEvent @event, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(IEventMessage<TEvent> message) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(IEventMessage<TEvent> message, string topic) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }

        public Task PublishAsync<TEvent>(IEventMessage<TEvent> message, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent
        {
            return Task.CompletedTask;
        }
    }
}
