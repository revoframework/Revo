using System;
using System.Threading.Tasks;
using EasyNetQ;
using Revo.Core.Events;

namespace Revo.EasyNetQ
{
    public interface IEasyNetQBus
    {
        Task PublishAsync<TEvent>(TEvent @event) where TEvent : IEvent;
        Task PublishAsync<TEvent>(TEvent @event, string topic) where TEvent : IEvent;
        Task PublishAsync<TEvent>(TEvent @event, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent;
        Task PublishAsync<TEvent>(IEventMessage<TEvent> message) where TEvent : IEvent;
        Task PublishAsync<TEvent>(IEventMessage<TEvent> message, string topic) where TEvent : IEvent;
        Task PublishAsync<TEvent>(IEventMessage<TEvent> message, Action<IPublishConfiguration> publishConfiguration) where TEvent : IEvent;
    }
}
